using UnityEngine;
using System.Collections;

/// <summary>
/// Handles level finish logic with instant UI lockdown to prevent menu leaks.
/// (Menü sızıntılarını önlemek için anında UI kilitlemeli bölüm bitiş mantığını yönetir.)
/// </summary>
public class FinishPoint : MonoBehaviour, IResettable
{
    // Diğer scriptlerin (PauseManager gibi) görebilmesi için static yapıldı
    public static bool IsLevelFinishing { get; private set; } = false;
    public static bool isPlayerInZone = false;
    public static bool isFinishBlocked = false;

    private bool _isProcessing = false;
    private Rigidbody2D _playerRb;

    /// <summary>
    /// Registers to the LevelManager and enforces a clean state on startup.
    /// (LevelManager'a kayıt olur ve başlangıçta temiz bir durum dayatır.)
    /// </summary>
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // Başlangıçta statik kilitlerin kapalı olduğundan kesinlikle emin ol
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
    /// Triggered when the player enters the finish zone.
    /// (Oyuncu bitiş alanına girdiğinde tetiklenir.)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER) && !_isProcessing && !isFinishBlocked)
        {
            // --- ALTIN VURUŞ: ANINDA KİLİTLE ---
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
        if (other.CompareTag(Constants.TAG_PLAYER) && !IsLevelFinishing)
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

        // Karakteri durdur
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (PlayerController.Instance != null)
            PlayerController.Instance.canMove = false;

        if (SoundManager.Instance != null)
            SoundManager.PlayThemeSFX(SFXType.DoorPass);

        // Bir frame bekle ve sahneyi yükle
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

