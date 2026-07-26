using UnityEngine;

/// <summary>
/// Karakterin belirli bir noktadan diğerine giderken silikleşmesini (saydamlaşmasını) sağlayan mekanik.
/// Kendi oyununun PlayerController sistemine ve IResettable mantığına uyumludur.
/// </summary>
public class PlayerFadeMechanic : MonoBehaviour, IResettable
{
    [Header("Referans Noktaları")]
    [Tooltip("Silikleşmenin başlayacağı, karakterin tam görünür olduğu nokta")]
    public Transform startPoint;
    [Tooltip("Karakterin tamamen silikleşeceği (görünmez olacağı) nokta")]
    public Transform endPoint;

    [Header("Saydamlık Limitleri")]
    [Tooltip("Başlangıç noktasındaki saydamlık (1 = Tam Görünür)")]
    [Range(0f, 1f)] public float startAlpha = 1f;
    [Tooltip("Bitiş noktasındaki saydamlık (0 = Tamamen Görünmez)")]
    [Range(0f, 1f)] public float endAlpha = 0f;

    private Transform playerTransform;
    private SpriteRenderer playerSprite;
    private bool isPlayerFound = false;

    void Start()
    {
        // LevelManager'a bu mekaniği kayıt et (Öldüğünde vs. sıfırlanması için)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        FindPlayer();
    }

    private void FindPlayer()
    {
        // Player'ı tag üzerinden otomatik bul
        GameObject playerObj = GameObject.FindGameObjectWithTag(Constants.TAG_PLAYER);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerSprite = playerObj.GetComponent<SpriteRenderer>();

            if (playerSprite != null)
            {
                isPlayerFound = true;
            }
        }
    }

    void Update()
    {
        // Karakter bulunamadıysa (veya silindiyse) tekrar bulmaya çalış
        if (!isPlayerFound)
        {
            FindPlayer();
            return;
        }

        if (startPoint == null || endPoint == null) return;

        // Başlangıç ve bitiş noktası arasındaki yön vektörü
        Vector3 pathVector = endPoint.position - startPoint.position;
        // Karakterin başlangıç noktasına göre konumu
        Vector3 characterVector = playerTransform.position - startPoint.position;

        // Nokta çarpım (Dot product) ile karakterin yol üzerindeki izdüşümünü alıyoruz.
        float progress = Vector3.Dot(characterVector, pathVector.normalized) / pathVector.magnitude;

        // Değerin 0 ile 1 arasında kalmasını sağlıyoruz
        progress = Mathf.Clamp01(progress);

        // İlerlemeye göre yeni saydamlık (alpha) değerini hesapla
        float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

        // Hesaplanmış Alpha'yı karakterin SpriteRenderer'ına uygula
        Color c = playerSprite.color;
        c.a = currentAlpha;
        playerSprite.color = c;
    }

    // Karakter öldüğünde veya bölüm sıfırlandığında IResettable üzerinden çağrılır
    public void ResetMechanic()
    {
        if (isPlayerFound && playerSprite != null)
        {
            Color c = playerSprite.color;
            c.a = startAlpha; // Başlangıç saydamlığına (genelde 1) geri döndür
            playerSprite.color = c;
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}
