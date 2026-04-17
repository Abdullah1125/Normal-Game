using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kapý koduna ulaþmaya çalýþýyoruz
            if (GateController.Instance != null)
            {
                GateController.Instance.RegisterKeyCollected();

                // Ses çal
                if (SoundManager.instance != null)
                    SoundManager.PlaySFX(SoundManager.instance.keySound);

                // Anahtarý gizle
                gameObject.SetActive(false);
                Debug.Log("Anahtar baþarýyla toplandý!");
            }
            else
            {
                Debug.LogError("Hata: Sahnede GateController bulunamadý!");
            }
        }
    }

    public void ResetKey()
    {
        gameObject.SetActive(true);
    }
}