using UnityEngine;

/// <summary>
/// Sabit ekranlý oyunlarda, belirlenen temel oyun alanýnýn (Core Area)
/// tüm cihaz ekranlarýnda kesilmeden tam olarak görünmesini saðlar.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedScreenFitter : MonoBehaviour
{
    [Header("Core Area Bounds (Temel Alan Sýnýrlarý)")]
    [Tooltip("Oynanýþ platformlarýnýn dýþýna çýkmayacak þekilde ayarlanan 4 sýnýr noktasý.")]
    public Transform[] corePoints = new Transform[4];

    private Camera cam;

    /// <summary>
    /// Kamera referansýný alýr ve ortografik moda zorlar.
    /// </summary>
    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            cam.orthographic = true;
        }
    }

    /// <summary>
    /// Oyun baþlarken kamerayý ekran oranýna göre hizalar.
    /// </summary>
    private void Start()
    {
        FitCoreArea();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editörde çözünürlük deðiþtirildiðinde kamerayý test etmek için dinamik günceller.
    /// Sadece Unity Editor içinde çalýþýr, mobil cihaza build alýndýðýnda performansý etkilemez.
    /// </summary>
    private void Update()
    {
        FitCoreArea();
    }
#endif

    /// <summary>
    /// Belirlenen noktalarý kapsayan alaný hesaplar ve
    /// kameranýn boyutunu bu alaný kesinlikle kesmeyecek þekilde ayarlar.
    /// </summary>
    private void FitCoreArea()
    {
        if (corePoints == null || corePoints.Length == 0) return;

        // Noktalarýn sýnýrlarýný (Bounds) belirle
        Bounds bounds = new Bounds(corePoints[0].position, Vector3.zero);
        for (int i = 1; i < corePoints.Length; i++)
        {
            if (corePoints[i] != null)
            {
                bounds.Encapsulate(corePoints[i].position);
            }
        }

        // Kamerayý tam merkeze sabitle
        Vector3 centerPos = bounds.center;
        centerPos.z = transform.position.z;
        transform.position = centerPos;

        // Ekran oranýna göre kameranýn yatay ve dikey gereksinimlerini hesapla
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetSizeY = bounds.extents.y;
        float targetSizeX = bounds.extents.x / screenAspect;

        // Hiçbir oynanýþ objesinin dýþarýda kalmamasý için en büyük deðeri (Max) seç
        cam.orthographicSize = Mathf.Max(targetSizeY, targetSizeX);
    }
}