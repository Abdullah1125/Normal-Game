using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    // 2D bir tetikleyici alana (Trigger) girildiðinde otomatik çalýþýr
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            SoundManager.PlaySFX(SoundManager.instance.doorPassSound);
            LevelManager.Instance.NextLevel();
        }
    }
}