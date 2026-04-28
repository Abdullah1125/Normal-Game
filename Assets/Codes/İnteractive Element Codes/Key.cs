using UnityEngine;

public class Key : MonoBehaviour , IResettable
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
                    SoundManager.PlayThemeSFX(SFXType.Key);

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
    void Start()
    {
        // Register to LevelManager (LevelManager'a kendini kaydettir)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }
    public void ResetMechanic()
    {
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        // Obje silinirken LevelManager'ýn listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // Eðer LevelManager'da RemoveResettable fonksiyonu yoksa aþaðýya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}