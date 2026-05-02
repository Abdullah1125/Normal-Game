using UnityEngine;

/// <summary>
/// Allows the gate to be dragged upwards only. Disables the normal gate on start.
/// (Kapýnýn sadece yukarý kaydýrýlmasýna izin verir. Baþlangýçta normal kapýyý kapatýr.)
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
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mouseWorldPos;
    }

    private void OnMouseDrag()
    {
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
}