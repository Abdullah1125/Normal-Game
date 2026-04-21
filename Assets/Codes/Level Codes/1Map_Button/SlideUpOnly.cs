using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlideUpOnly : MonoBehaviour
{
    [Header("Slide Settings (Kaydýrma Ayarlarý)")]
    [Tooltip("Obje yukarý doðru en fazla kaç birim kalkabilsin?")]
    public float maxUpwardLimit = 3f;

    private Vector3 startPos;
    private Vector3 dragOffset;
    private Camera mainCam;

    /// <summary>
    /// Initializes settings and automatically disables the old gate object in the scene.
    /// (Ayarlarý baþlatýr ve sahnedeki eski kapý objesini otomatik bulup iþlevsiz hale getirir.)
    /// </summary>
    void Start()
    {
        startPos = transform.position;
        mainCam = Camera.main;

        // Sahnedeki eski kapýyý otomatik olarak bul (Unity 6 standartlarýnda en hýzlý arama yöntemi)
        GateController oldLogic = Object.FindFirstObjectByType<GateController>();

        if (oldLogic != null)
        {
            GameObject oldGateObject = oldLogic.gameObject;

            // 1. Kodu iptal et (Hareket etmesin)
            oldLogic.enabled = false;

            // 2. Görseli kapat (Görünmez olsun)
            SpriteRenderer sr = oldGateObject.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            // 3. Fiziði kapat (Ýçinden geçilebilsin, çarpýþmasýn)
            Collider2D col = oldGateObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
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

        // Yeni Y pozisyonunu hesapla
        float newY = mouseWorldPos.y + dragOffset.y;

        // Aþaðý inmesini ve max limiti geçmesini engelle
        float clampedY = Mathf.Clamp(newY, startPos.y, startPos.y + maxUpwardLimit);

        // X ve Z pozisyonunu kilitle, sadece Y deðiþsin
        transform.position = new Vector3(startPos.x, clampedY, startPos.z);
    }
}