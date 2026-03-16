using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    // 2D bir tetikleyici alana (Trigger) girildiðinde otomatik çalýþýr
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
           
            LevelManager.Instance.NextLevel();
        }
    }
}