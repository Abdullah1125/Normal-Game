using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles level finish logic with instant UI lockdown to prevent menu leaks.
/// Updates the gate visual dynamically from a list based on the custom LevelManager data (0-indexed).
/// Plays specific milestone sounds based on the next theme index.
/// (Menü sızıntılarını önlemek için anında UI kilitlemeli bölüm bitiş mantığını yönetir. Kapı görselini 0 endeksli LevelManager verisine göre günceller. Bir sonraki temanın özel sesini çalar.)
/// </summary>
public class FinishPoint : MonoBehaviour, IResettable
{
    [Header("Visual Settings (Görsel Ayarlar)")]
    public SpriteRenderer gateRenderer;
    public Sprite normalGateSprite;

    [Tooltip("List of special gate sprites for milestone levels (12, 24, 36...). \n(12, 24, 36 gibi özel bölümlerde sırasıyla çıkacak kapı görselleri listesi.)")]
    public List<Sprite> specialGateSprites;

    public static bool IsLevelFinishing { get; private set; } = false;
    public static bool isPlayerInZone = false;
    public static bool isFinishBlocked = false;

    private bool _isProcessing = false;
    private Rigidbody2D _playerRb;

    /// <summary>
    /// Subscribes to the LevelManager's start event to ensure visual updates on every level transition.
    /// (Her seviye geçişinde görsel güncelleme sağlamak için LevelManager'ın başlangıç olayına abone olur.)
    /// </summary>
    private void OnEnable()
    {
        LevelManager.OnLevelStarted += SetupGateVisual;
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks and errors.
    /// (Hataları ve bellek sızıntılarını önlemek için olay aboneliğinden çıkar.)
    /// </summary>
    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= SetupGateVisual;
    }

    /// <summary>
    /// Registers to the LevelManager, updates visual state, and enforces a clean state on startup.
    /// (LevelManager'a kayıt olur, görsel durumu günceller ve başlangıçta temiz bir durum dayatır.)
    /// </summary>
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        SetupGateVisual();
        ResetMechanic();
    }

    /// <summary>
    /// Unregisters from the LevelManager to prevent memory leaks.
    /// (Bellek sızıntısını önlemek için sistem kaydını siler.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    /// <summary>
    /// Updates the gate visual based on the current level.
    /// (Mevcut bölüme göre kapı görselini günceller.)
    /// </summary>
    private void SetupGateVisual()
    {
        if (gateRenderer == null) return;

        int currentLevelID = 0;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            currentLevelID = LevelManager.Instance.activeLevel.levelID;
        }

        // 12, 24, 36. leveller (ID 11, 23, 35) için görsel kontrolü
        if (currentLevelID > 0 && (currentLevelID + 1) % 12 == 0)
        {
            if (specialGateSprites != null && specialGateSprites.Count > 0)
            {
                int specialIndex = ((currentLevelID + 1) / 12) - 1;

                if (specialIndex < specialGateSprites.Count)
                {
                    gateRenderer.sprite = specialGateSprites[specialIndex];
                }
                else
                {
                    if (normalGateSprite != null) gateRenderer.sprite = normalGateSprite;
                }
            }
        }
        else
        {
            if (normalGateSprite != null) gateRenderer.sprite = normalGateSprite;
        }
    }

    /// <summary>
    /// Triggered when the player enters the finish zone.
    /// (Oyuncu bitiş alanına girdiğinde tetiklenir.)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isProcessing && !isFinishBlocked)
        {
            IsLevelFinishing = true;

            if (UIManager.Instance != null)
                UIManager.Instance.SetHUDBlock(true);

            isPlayerInZone = true;
            _playerRb = other.GetComponent<Rigidbody2D>();

            StartCoroutine(FinishSequence(_playerRb));
        }
    }

    /// <summary>
    /// Triggered when the player leaves the zone (before processing starts).
    /// (İşlem başlamadan önce oyuncu alandan çıkarsa tetiklenir.)
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !IsLevelFinishing)
        {
            isPlayerInZone = false;
            _playerRb = null;
        }
    }

    /// <summary>
    /// Executes the final sequence, handles specific theme sounds, and triggers scene transition.
    /// (Final sekansını yürütür, özel tema seslerini ayarlar ve sahne geçişini tetikler.)
    /// </summary>
    private IEnumerator FinishSequence(Rigidbody2D playerRb)
    {
        _isProcessing = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (PlayerController.Instance != null)
            PlayerController.Instance.canMove = false;

        // --- SES YÖNETİMİ (SOUND MANAGEMENT) ---
        if (SoundManager.Instance != null && LevelManager.Instance != null)
        {
            int currentLevelID = LevelManager.Instance.activeLevel.levelID;

            // Eğer özel bölümdeysek (12, 24, 36...)
            if ((currentLevelID + 1) % 12 == 0)
            {
                // Bir sonraki temanın indeksini hesapla
                int nextThemeIndex = ((currentLevelID + 1) / 12);

                // Liste aşımını (Out of Bounds) önle
                if (nextThemeIndex < SoundManager.Instance.themeAudios.Length)
                {
                    // Bir sonraki temanın kapı sesini al ve doğrudan çal
                    AudioClip specialClip = SoundManager.Instance.themeAudios[nextThemeIndex].doorPassSound;
                    SoundManager.PlayClipDirectly(specialClip);
                }
                else
                {
                    // Temalar bittiyse varsayılanı çal
                    SoundManager.PlayThemeSFX(SFXType.DoorPass);
                }
            }
            else
            {
                // Normal bölümlerde standart temaya uygun çal
                SoundManager.PlayThemeSFX(SFXType.DoorPass);
            }
        }

        yield return new WaitForEndOfFrame();

        if (LevelManager.Instance != null)
            LevelManager.Instance.NextLevel();
    }

    /// <summary>
    /// Resets the finish state and releases all global UI blocks.
    /// (Bitiş durumunu sıfırlar ve tüm global UI kilitlerini kaldırır.)
    /// </summary>
    public void ResetMechanic()
    {
        IsLevelFinishing = false;
        _isProcessing = false;
        isPlayerInZone = false;
        _playerRb = null;
        isFinishBlocked = false;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDBlock(false);
            UIManager.Instance.SetPauseBlock(false);
        }
    }
}