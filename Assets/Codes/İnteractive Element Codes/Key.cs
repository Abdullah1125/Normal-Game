using UnityEngine;

public class Key : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D other)
    {
      
        if (other.CompareTag("Player"))
        {
            if (LevelManager.Instance != null && !LevelManager.Instance.activeLevel.isActive)
            {
               
                return;
            }

            if (GateController.Instance != null)
            {
                GateController.Instance.RegisterKeyCollected();
            }

            SoundManager.PlaySFX(SoundManager.instance.keySound);
            gameObject.SetActive(false);

            Debug.Log("Key collected, gate is opening!");
        }
    }

    // Seviye yeniden baþladýðýnda anahtarý tekrar görünür yapmak için kullanýlýr
    public void ResetKey()
    {
        gameObject.SetActive(true);
    }
}