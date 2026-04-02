using UnityEngine;

public class DragAndDropControl : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;

 
    public string playerTag = "Player"; 

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag(playerTag))
            {
               
                rb = hit.collider.GetComponent<Rigidbody2D>();

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