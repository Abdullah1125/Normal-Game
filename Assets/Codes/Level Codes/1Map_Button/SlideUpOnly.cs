using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlideUpOnly : MonoBehaviour , IResettable
{
    [Header("Slide Settings (Kaydýrma Ayarlarý)")]
    [Tooltip("Maximum upward limit for the object. (Obje yukarý doðru en fazla kaç birim kalkabilsin?)")]
    public float maxUpwardLimit = 3f;

    private Vector3 startPos;
    private Vector3 dragOffset;
    private Camera mainCam;

    /// <summary>
    /// Records the starting position immediately when the object is instantiated.
    /// (Obje sahneye düþtüðü ilk milisaniye baþlangýç pozisyonunu hafýzaya alýr.)
    /// </summary>
    void Awake()
    {
        startPos = transform.position;
    }

    /// <summary>
    /// Initializes settings and disables the old gate object automatically.
    /// (Ayarlarý baþlatýr ve sahnedeki eski kapý objesini otomatik iþlevsiz hale getirir.)
    /// </summary>
    void Start()
    {

        // Register to LevelManager (LevelManager'a kendini kaydettir)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        mainCam = Camera.main;
      
        GateController oldLogic = GateController.Instance;

        if (oldLogic != null)
        {
            GameObject oldGateObject = oldLogic.gameObject;

            // Disable logic, visual, and physics (Kodu, görseli ve fiziði kapat)
            oldLogic.enabled = false;

            SpriteRenderer sr = oldGateObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            Collider2D col = oldGateObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    /// <summary>
    /// Resets the gate to its original starting position via IResettable interface.
    /// (IResettable arayüzü üzerinden kapýyý orijinal baþlangýç pozisyonuna geri ýþýnlar.)
    /// </summary>
    public void ResetMechanic() // SÝHÝR 3: Ýsim standartlaþtý!
    {
        transform.position = startPos;
    }

    /// <summary>
    /// Calculates the offset between the object center and touch point.
    /// (Dokunulan nokta ile objenin merkezi arasýndaki mesafeyi hesaplar.)
    /// </summary>
    private void OnMouseDown()
    {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        dragOffset = transform.position - mouseWorldPos;
    }

    /// <summary>
    /// Moves the object strictly upwards within the maximum limit.
    /// (Objeyi, maksimum sýnýr içerisinde sadece yukarý doðru hareket ettirir.)
    /// </summary>
    private void OnMouseDrag()
    {
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);

        // Calculate new Y position (Yeni Y pozisyonunu hesapla)
        float newY = mouseWorldPos.y + dragOffset.y;

        // Prevent it from going below start or above limit (Aþaðý inmesini ve limiti geçmesini engelle)
        float clampedY = Mathf.Clamp(newY, startPos.y, startPos.y + maxUpwardLimit);

        // Lock X and Z, only update Y (X ve Z'yi kilitle, sadece Y'yi güncelle)
        transform.position = new Vector3(startPos.x, clampedY, startPos.z);
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