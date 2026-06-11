using UnityEngine;

public class Key : MonoBehaviour , IResettable
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Kapı koduna ulaşmaya çalışıyoruz
            if (GateController.Instance != null)
            {
                GateController.Instance.RegisterKeyCollected();

                // Ses çal
                if (SoundManager.Instance != null)
                    SoundManager.PlayThemeSFX(SFXType.Key);

                // Anahtarı gizle
                gameObject.SetActive(false);
              
            }
            else
            {
             
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
        // Obje silinirken LevelManager'ın listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // Eğer LevelManager'da RemoveResettable fonksiyonu yoksa aşağıya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}

