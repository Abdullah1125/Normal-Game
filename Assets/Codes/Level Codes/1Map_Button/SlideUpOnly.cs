using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Allows the gate to be dragged upwards only with visual feedback.
/// Disables the normal gate on start and stops effects at movement limits.
/// (Kapýnýn sadece yukarý kaydýrýlmasýna izin verir ve görsel geri bildirim saðlar. Baþlangýçta normal kapýyý kapatýr ve hareket sýnýrlarýnda efektleri durdurur.)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SlideUpOnly : MonoBehaviour, IResettable
{
    [Header("Slide Settings (Kaydýrma Ayarlarý)")]
    public float maxUpwardLimit = 3f;

    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem dragEffect; // Sürükleme sýrasýnda oynatýlacak efekt

    private Vector3 startPos;
    private Vector3 dragOffset;
    private Camera mainCam;
    private bool _didDisableNormalGate = false;

    /// <summary>
    /// Caches the initial position and detaches the particle system.
    /// (Baþlangýç pozisyonunu önbelleðe alýr ve parçacýk sistemini objeden ayýrýr.)
    /// </summary>
    void Awake()
    {
        startPos = transform.position;

        if (dragEffect != null)
        {
            dragEffect.transform.SetParent(null);
        }
    }

    /// <summary>
    /// Registers to the level manager and manages the normal gate state.
    /// (Seviye yöneticisine kaydolur ve normal kapý durumunu yönetir.)
    /// </summary>
    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        mainCam = Camera.main;

        if (GateController.Instance != null && GateController.Instance.gameObject.activeSelf)
        {
            GateController.Instance.gameObject.SetActive(false);
            _didDisableNormalGate = true;
        }
    }

    /// <summary>
    /// Resets the gate to its initial state and stops effects.
    /// (Kapýyý baþlangýç durumuna döndürür ve efektleri durdurur.)
    /// </summary>
    public void ResetMechanic()
    {
        transform.position = startPos;
        StopEffect();
    }

    /// <summary>
    /// Initiates drag interaction and triggers visual effects.
    /// (Sürükleme etkileþimini baþlatýr ve görsel efektleri tetikler.)
    /// </summary>
    private void OnMouseDown()
    {
        if (Time.timeScale == 0f || IsPointerOverUI()) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mouseWorldPos;

        PlayEffect();
    }

    /// <summary>
    /// Processes upward movement, applies limits, and manages particle flow at boundaries.
    /// (Yukarý yönlü hareketi iþler, limitleri uygular ve sýnýrlarda parçacýk akýþýný yönetir.)
    /// </summary>
    private void OnMouseDrag()
    {
        if (Time.timeScale == 0f)
        {
            StopEffect();
            return;
        }

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        float newY = mouseWorldPos.y + dragOffset.y;

        // Limit hesaplamalarý
        float limitY = startPos.y + maxUpwardLimit;
        float clampedY = Mathf.Clamp(newY, startPos.y, limitY);

        transform.position = new Vector3(startPos.x, clampedY, startPos.z);

        // --- YENÝ EKLENEN KONTROL: Sýnýr Kontrolü ---
        // Kapý en üst limite ulaþtýysa (veya en alta dayandýysa) efekti kes
        if (clampedY >= limitY || clampedY <= startPos.y)
        {
            StopEffect();
        }
        else
        {
            PlayEffect(); // Aradayken oynatmaya devam et
        }
    }

    /// <summary>
    /// Stops the visual feedback when interaction ends.
    /// (Etkileþim bittiðinde görsel geri bildirimi durdurur.)
    /// </summary>
    private void OnMouseUp()
    {
        StopEffect();
    }

    /// <summary>
    /// Plays the assigned particle system if it exists.
    /// (Atanmýþ bir parçacýk sistemi varsa oynatýr.)
    /// </summary>
    private void PlayEffect()
    {
        if (dragEffect != null && !dragEffect.isPlaying)
        {
            dragEffect.Play();
        }
    }

    /// <summary>
    /// Stops the assigned particle system safely.
    /// (Atanmýþ parçacýk sistemini güvenli bir þekilde durdurur.)
    /// </summary>
    private void StopEffect()
    {
        if (dragEffect != null && dragEffect.isPlaying)
        {
            dragEffect.Stop();
        }
    }

    /// <summary>
    /// Cleans up references and reactivates the normal gate.
    /// (Referanslarý temizler ve normal kapýyý tekrar aktifleþtirir.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }

        if (_didDisableNormalGate && GateController.Instance != null)
        {
            GateController.Instance.gameObject.SetActive(true);
        }

        if (dragEffect != null && dragEffect.gameObject != null)
        {
            Destroy(dragEffect.gameObject);
        }
    }

    /// <summary>
    /// Checks if the pointer is currently interacting with UI elements.
    /// (Ýmlecin þu anda arayüz elemanlarýyla etkileþimde olup olmadýðýný kontrol eder.)
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;

        return false;
    }
}