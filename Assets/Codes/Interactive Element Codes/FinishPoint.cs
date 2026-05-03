using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles level finish logic with instant UI lockdown to prevent menu leaks.
/// Updates the gate visual dynamically from a list based on the custom LevelManager data (0-indexed).
/// Listens to LevelManager events to refresh visuals during transitions.
/// (Menü sızıntılarını önlemek için anında UI kilitlemeli bölüm bitiş mantığını yönetir. Kapı görselini 0 endeksli LevelManager verisine göre bir listeden dinamik günceller. Geçişlerde görselleri yenilemek için LevelManager'ı dinler.)
/// </summary>
public class FinishPoint : MonoBehaviour, IResettable
{
    [Header("Visual Settings (Görsel Ayarlar)")]
    public SpriteRenderer gateRenderer; // Kapının SpriteRenderer bileşeni
    public Sprite normalGateSprite; // Normal levellerde görünecek kapı

    [Tooltip("List of special gate sprites for milestone levels (12, 24, 36...). \n(12, 24, 36 gibi özel bölümlerde sırasıyla çıkacak kapı görselleri listesi.)")]
    public List<Sprite> specialGateSprites; // Liste olarak özel kapılar

    public static bool IsLevelFinishing { get; private set; } = false;
    public static bool isPlayerInZone = false;
    public static bool isFinishBlocked = false;

    private bool _isProcessing = false;
    private Rigidbody2D _playerRb;

    // --- GÖRSEL YENİLEME SİHRİ BURADA ---
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
    // ------------------------------------

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
    /// Determines if the current level (0-indexed) is a milestone and applies the correct sprite from the list.
    /// (Mevcut bölümün (0 endeksli) bir kilometre taşı olup olmadığını belirler ve listeden doğru görseli uygular.)
    /// </summary>
    private void SetupGateVisual()
    {
        if (gateRenderer == null) return;

        int currentLevelID = 0;

        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            currentLevelID = LevelManager.Instance.activeLevel.levelID;
        }

        // Kural: ID'ler 0'dan başladığı için (+1) ekleyerek 12'ye göre mod (kalan) alıyoruz.
        if (currentLevelID > 0 && (currentLevelID + 1) % 12 == 0)
        {
            if (specialGateSprites != null && specialGateSprites.Count > 0)
            {
                // Hangi özel kapının sırası geldiğini hesapla (12. bölüm = index 0, 24. bölüm = index 1 vb.)
                int specialIndex = ((currentLevelID + 1) / 12) - 1;

                // Başa sarma yok. Sadece listede o sıraya ait görsel varsa kullan.
                if (specialIndex < specialGateSprites.Count)
                {
                    gateRenderer.sprite = specialGateSprites[specialIndex];
                }
                else
                {
                    // Liste aşıldıysa (örneğin 3 görsel var ama 48. bölüme gelindiyse) normal kapıya dön.
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
    /// Executes the final sequence and triggers scene transition.
    /// (Final sekansını yürütür ve sahne geçişini tetikler.)
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

        if (SoundManager.Instance != null)
            SoundManager.PlayThemeSFX(SFXType.DoorPass);

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