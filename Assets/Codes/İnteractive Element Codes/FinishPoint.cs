using UnityEngine;
using System.Collections;

public class FinishPoint : MonoBehaviour
{
    private bool _isProcessing = false;


    public static bool isPlayerInZone = false;

    private Rigidbody2D _playerRb;
    public static bool isFinishBlocked = false;

    private void OnEnable() { LevelManager.OnLevelStarted += ResetDoor; }
    private void OnDisable() { LevelManager.OnLevelStarted -= ResetDoor; }

    private void ResetDoor()
    {
        _isProcessing = false;
        isPlayerInZone = false;
        _playerRb = null;
        isFinishBlocked = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            _playerRb = other.GetComponent<Rigidbody2D>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            _playerRb = null;
        }
    }

    private void Update()
    {
        if (isPlayerInZone && !_isProcessing && !isFinishBlocked && _playerRb != null)
        {
            StartCoroutine(FinishSequence(_playerRb));
        }
    }

    private IEnumerator FinishSequence(Rigidbody2D playerRb)
    {
        _isProcessing = true;
        playerRb.linearVelocity = Vector2.zero;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        if (SoundManager.instance != null) SoundManager.PlayThemeSFX(SFXType.DoorPass);

        LevelManager.Instance.NextLevel();
        yield return null;
    }
}