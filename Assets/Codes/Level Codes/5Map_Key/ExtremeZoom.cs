using UnityEngine;

/// <summary>
/// Kamerayý oyuncuya yaklaþtýrýr ve pürüzsüz bir þekilde takip etmesini saðlar.
/// (Zooms the camera to the player and enables smooth follow.)
/// </summary>
public class ExtremeZoom : MonoBehaviour
{
    [Header("Zoom Settings (Zoom Ayarlarý)")]
    [SerializeField] private float targetZoomSize = 2f;
    [SerializeField] private float smoothTime = 0.3f;

    private Camera mainCam;
    private MonoBehaviour fitter;
    private Transform player;
    private Vector3 velocity = Vector3.zero;

    // Kameranýn orijinal uzaklýðýný hafýzada tutmak için
    private float originalZoomSize;

    /// <summary>
    /// Kamera referansýný alýr ve hedef bileþeni önbelleðe kaydeder.
    /// </summary>
    private void Awake()
    {
        mainCam = Camera.main;

        if (mainCam != null)
        {
            // Orijinal zoom deðerini sakla
            originalZoomSize = mainCam.orthographicSize;

            // Tip (Type) eriþimi olmadýðý durumlarda en güvenli string tabanlý arama
            Component component = mainCam.GetComponent("FixedScreenFitter");
            if (component != null)
            {
                fitter = component as MonoBehaviour;
            }
        }
    }

    /// <summary>
    /// Gerekli atamalarý yapar, ekran sýnýrlayýcýyý kapatýr ve kamerayý hedefe odaklar.
    /// </summary>
    private void Start()
    {
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;

        if (fitter != null)
            fitter.enabled = false;

        // Kamerayý anýnda karaktere ýþýnla ve zoom yap
        if (mainCam != null && player != null)
        {
            AlignCamera();
            mainCam.orthographicSize = targetZoomSize;
        }
    }

    /// <summary>
    /// Her karede kameranýn oyuncuyu pürüzsüz bir þekilde takip etmesini saðlar.
    /// </summary>
    private void LateUpdate()
    {
        if (mainCam == null || player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, player.position.y, mainCam.transform.position.z);
        mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, targetPos, ref velocity, smoothTime);
    }

    /// <summary>
    /// Script aktifleþtiðinde oyuncu sýfýrlanma olayýna abone olur.
    /// </summary>
    private void OnEnable()
    {
        PlayerController.OnPlayerReset += AlignCamera;
    }

    /// <summary>
    /// Script kapandýðýnda abonelikleri iptal eder ve kamera ayarlarýný eski haline döndürür.
    /// </summary>
    private void OnDisable()
    {
        PlayerController.OnPlayerReset -= AlignCamera;

        // Fitter'ý geri aç ve kameranýn zoom deðerini eski haline getir
        if (fitter != null) fitter.enabled = true;
        if (mainCam != null) mainCam.orthographicSize = originalZoomSize;
    }

    /// <summary>
    /// Kameranýn X ve Y pozisyonunu oyuncunun konumuna anýnda eþitler.
    /// </summary>
    private void AlignCamera()
    {
        if (mainCam != null && player != null)
        {
            mainCam.transform.position = new Vector3(player.position.x, player.position.y, mainCam.transform.position.z);
        }
    }
}