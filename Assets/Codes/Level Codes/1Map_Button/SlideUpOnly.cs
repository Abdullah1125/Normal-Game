using UnityEngine;
using UnityEngine.EventSystems; // UI algýlamasý için eklendi

/// <summary>
/// Allows the gate to be dragged upwards only. Disables the normal gate on start.
/// Includes pause and UI click-through protections.
/// (Kapýnýn sadece yukarý kaydýrýlmasýna izin verir. Baþlangýçta normal kapýyý kapatýr. Duraklatma ve arayüz týklama korumalarýný içerir.)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SlideUpOnly : MonoBehaviour, IResettable
{
    [Header("Slide Settings (Kaydýrma Ayarlarý)")]
    public float maxUpwardLimit = 3f;

    private Vector3 startPos;
    private Vector3 dragOffset;
    private Camera mainCam;
    private bool _didDisableNormalGate = false;

    void Awake()
    {
        startPos = transform.position;
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        mainCam = Camera.main;

        // --- SÝHÝR BURADA: Normal kapýyý (Singleton) komple kapat ---
        if (GateController.Instance != null && GateController.Instance.gameObject.activeSelf)
        {
            GateController.Instance.gameObject.SetActive(false);
            _didDisableNormalGate = true;
            Debug.Log("JÝLET TROLL: Normal kapý komple gizlendi.");
        }
    }

    public void ResetMechanic()
    {
        transform.position = startPos;
    }

    private void OnMouseDown()
    {
        // JÝLET GÝBÝ KORUMA: Oyun durmuþsa veya bir UI paneline týklanýyorsa kapýyý tutmayý reddet!
        if (Time.timeScale == 0f) return;
        if (IsPointerOverUI()) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mouseWorldPos;
    }

    private void OnMouseDrag()
    {
        // JÝLET GÝBÝ KORUMA: Oyun durmuþsa sürüklemeyi anýnda kes!
        if (Time.timeScale == 0f) return;
        if (IsPointerOverUI()) return;

        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        float newY = mouseWorldPos.y + dragOffset.y;

        // Limitleri koru (Aþaðý inemez, max limiti geçemez)
        float clampedY = Mathf.Clamp(newY, startPos.y, startPos.y + maxUpwardLimit);

        transform.position = new Vector3(startPos.x, clampedY, startPos.z);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }

        // --- ALTIN VURUÞ: Level bitince normal kapýyý geri uyandýr ---
        if (_didDisableNormalGate && GateController.Instance != null)
        {
            GateController.Instance.gameObject.SetActive(true);
            Debug.Log("JÝLET TROLL: Özel kapý bitti, normal kapý geri açýldý.");
        }
    }

    /// <summary>
    /// Checks if the user is currently touching/clicking a UI element.
    /// (Kullanýcýnýn þu anda bir arayüz elemanýna dokunup dokunmadýðýný kontrol eder.)
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // Fare veya Editor kontrolü
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        // Mobil dokunmatik kontrolü
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;

        return false;
    }
}