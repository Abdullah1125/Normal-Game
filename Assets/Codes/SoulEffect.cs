using UnityEngine;

public class SoulEffect : MonoBehaviour
{
    public float lifeTime = 3f;           // Objelerin yok edilmeden önceki bekleme süresi
    public float soulGravity = 15f;
    private bool hasTouchedGround = false; // Yere temas edilip edilmediði kontrolü
    private Rigidbody2D rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Dünya yer çekiminden etkilenmesin diye 0 yapýyoruz
        rb.gravityScale = 0f;
    }
    void FixedUpdate()
    {
     
        if (!hasTouchedGround)
        {
            rb.AddForce(Vector2.down * soulGravity, ForceMode2D.Force);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (!hasTouchedGround && (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground")))
        {
            hasTouchedGround = true;

          
            Destroy(gameObject, lifeTime);
        }
    }
}