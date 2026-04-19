using UnityEngine;
using System.Collections;

public class FinishPoint : MonoBehaviour
{
    private bool _isProcessing = false;

    private void OnEnable() { LevelManager.OnLevelStarted += ResetDoor; }
    private void OnDisable() { LevelManager.OnLevelStarted -= ResetDoor; }

    private void ResetDoor()
    {
        _isProcessing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isProcessing || !other.CompareTag("Player")) return;

        if (other.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            StartCoroutine(FinishSequence(rb));
        }
    }

    private IEnumerator FinishSequence(Rigidbody2D playerRb)
    {
        _isProcessing = true;

        playerRb.linearVelocity = Vector2.zero;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = false;
        }

        if (SoundManager.instance != null)
            SoundManager.PlaySFX(SoundManager.instance.doorPassSound);

        LevelManager.Instance.NextLevel();

        yield return null;
    }
}