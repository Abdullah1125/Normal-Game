using UnityEngine;
using System.Collections;

/// <summary>
/// Handles level finish logic with instant UI lockdown to prevent menu leaks.
/// (Menü sızıntılarını önlemek için anında UI kilitlemeli bölüm bitiş mantığını yönetir.)
/// </summary>
public class FinishPoint : MonoBehaviour
{
    // Diğer scriptlerin (PauseManager gibi) görebilmesi için static yaptık
    public static bool IsLevelFinishing { get; private set; } = false;

    private bool _isProcessing = false;
    public static bool isPlayerInZone = false;
    private Rigidbody2D _playerRb;
    public static bool isFinishBlocked = false;

    private void OnEnable() { LevelManager.OnLevelStarted += ResetDoor; }
    private void OnDisable() { LevelManager.OnLevelStarted -= ResetDoor; }

    /// <summary>
    /// Resets the finish state and releases all global UI blocks.
    /// (Bitiş durumunu sıfırlar ve tüm global UI kilitlerini kaldırır.)
    /// </summary>
    private void ResetDoor()
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isProcessing && !isFinishBlocked)
        {
            // --- ALTIN VURUŞ: ANINDA KİLİTLE ---
            IsLevelFinishing = true; // Statik kilidi kapat
            if (UIManager.Instance != null) UIManager.Instance.SetHUDBlock(true);

            isPlayerInZone = true;
            _playerRb = other.GetComponent<Rigidbody2D>();

            // Update'i beklemeden sekansı buradan da tetikleyebiliriz
            if (!_isProcessing) StartCoroutine(FinishSequence(_playerRb));
        }
    }

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
        if (_isProcessing) yield break;
        _isProcessing = true;

        // Karakteri durdur
        playerRb.linearVelocity = Vector2.zero;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        if (SoundManager.instance != null) SoundManager.PlayThemeSFX(SFXType.DoorPass);

        // Bir frame bekle ve sahneyi yükle
        yield return new WaitForEndOfFrame();
        LevelManager.Instance.NextLevel();
    }
}