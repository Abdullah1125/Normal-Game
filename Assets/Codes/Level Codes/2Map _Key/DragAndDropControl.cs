using UnityEngine;

public class DragAndDropControl : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;
    [Header("Settings")]
    public float grabRadius = 20;
    public LayerMask playerLayer;

 
    public string playerTag = "Player"; 

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            Collider2D hitCollider = Physics2D.OverlapCircle(mousePos, grabRadius , playerLayer);

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

        // 2. Sürükleme Devamı
        if (isDragging && Input.GetMouseButton(0) && rb != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            rb.transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, rb.transform.position.z);
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