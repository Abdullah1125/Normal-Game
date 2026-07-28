using UnityEngine;

/// <summary>
/// Karakterin belirli bir noktadan diğerine giderken silikleşmesini (saydamlaşmasını) sağlayan mekanik.
/// Kendi oyununun PlayerController sistemine ve IResettable mantığına uyumludur.
/// Mobil cihazlar için gereksiz bellek (GC) tahsisi önlenerek optimize edilmiştir.
/// </summary>
public class PlayerFadeMechanic : MonoBehaviour, IResettable
{
    [Header("Referans Noktaları")]
    [Tooltip("Silikleşmenin başlayacağı, karakterin tam görünür olduğu nokta.")]
    public Transform startPoint;
    
    [Tooltip("Karakterin tamamen silikleşeceği (görünmez olacağı) nokta.")]
    public Transform endPoint;

    [Header("Saydamlık Limitleri")]
    [Tooltip("Başlangıç noktasındaki saydamlık (1 = Tam Görünür).")]
    [Range(0f, 1f)] public float startAlpha = 1f;
    
    [Tooltip("Bitiş noktasındaki saydamlık (0 = Tamamen Görünmez).")]
    [Range(0f, 1f)] public float endAlpha = 0f;

    private Transform _playerTransform;
    private SpriteRenderer _playerSprite;
    private bool _isPlayerFound = false;
    private float _lastAlpha = -1f; // Aynı rengi tekrar tekrar atamamak için önbellek değeri

    private void Start()
    {
        // LevelManager'a bu mekaniği kayıt et (Karakter öldüğünde sıfırlanması için)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        FindPlayer();
    }

    /// <summary>
    /// Sahnedeki oyuncuyu bulur ve referanslarını önbelleğe (cache) alır.
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(Constants.TAG_PLAYER);
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _playerSprite = playerObj.GetComponent<SpriteRenderer>();

            if (_playerSprite != null)
            {
                _isPlayerFound = true;
                _lastAlpha = _playerSprite.color.a; // Başlangıç alpha değerini kaydet
            }
        }
    }

    private void Update()
    {
        // Karakter bulunamadıysa veya silindiyse tekrar bul
        if (!_isPlayerFound)
        {
            FindPlayer();
            return;
        }

        if (startPoint == null || endPoint == null) return;

        // Başlangıç ve bitiş noktası arasındaki vektör (yol)
        Vector3 pathVector = endPoint.position - startPoint.position;
        // Karakterin başlangıç noktasına göre konumu
        Vector3 characterVector = _playerTransform.position - startPoint.position;

        // Nokta çarpımı (Dot product) ile karakterin yol üzerindeki izdüşümünü alıyoruz
        float progress = Vector3.Dot(characterVector, pathVector.normalized) / pathVector.magnitude;

        // İlerleme değerini 0 ile 1 arasına sabitle
        progress = Mathf.Clamp01(progress);

        // İlerlemeye göre hedef saydamlık (alpha) değerini hesapla
        float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, progress);

        // Optimizasyon: Yalnızca saydamlık gerçekten değiştiğinde Color ataması yap (Gereksiz GC yaratımını önler)
        if (Mathf.Abs(_lastAlpha - currentAlpha) > 0.001f)
        {
            Color c = _playerSprite.color;
            c.a = currentAlpha;
            _playerSprite.color = c;
            _lastAlpha = currentAlpha;
        }
    }

    /// <summary>
    /// Karakter öldüğünde veya bölüm sıfırlandığında IResettable tarafından tetiklenir.
    /// </summary>
    public void ResetMechanic()
    {
        if (_isPlayerFound && _playerSprite != null)
        {
            Color c = _playerSprite.color;
            c.a = startAlpha; // Başlangıç saydamlığına geri döndür
            _playerSprite.color = c;
            _lastAlpha = startAlpha;
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
