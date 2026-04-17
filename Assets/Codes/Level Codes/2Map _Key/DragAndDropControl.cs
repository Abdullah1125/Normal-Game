using UnityEngine;

public class DragAndDropControl : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;

    [Header("Settings(Ayarlar)")]
    public float grabRadius = 1f; //1-2 arası daha idealdir
    public LayerMask playerLayer;
    public string playerTag = "Player";

    [Header("Sınırlar (Ekran/Harita)")]
    public bool useBoundaries = true; // Sınırlandırma aktif mi?
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    void Update()
    {
        // 1. Karakteri Yakalama
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapCircle(mousePos, grabRadius, playerLayer);

            if (hitCollider != null && hitCollider.CompareTag(playerTag))
            {
                rb = hitCollider.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    isDragging = true;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    offset = rb.transform.position - mousePos;
                    rb.linearVelocity = Vector2.zero;
                }
            }
        }

        // 2. Sürükleme 
        if (isDragging && Input.GetMouseButton(0) && rb != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Yeni pozisyonu hesapla
            float targetX = mousePos.x + offset.x;
            float targetY = mousePos.y + offset.y;

            //Pozisyonu sınırlar arasına hapset
            if (useBoundaries)
            {
                targetX = Mathf.Clamp(targetX, minX, maxX);
                targetY = Mathf.Clamp(targetY, minY, maxY);
            }

            rb.transform.position = new Vector3(targetX, targetY, rb.transform.position.z);
        }

        // 3. Bırakma
        if (Input.GetMouseButtonUp(0) && rb != null)
        {
            isDragging = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb = null;
        }
    }
}