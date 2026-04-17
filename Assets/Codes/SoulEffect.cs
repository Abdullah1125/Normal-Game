using UnityEngine;

public class SoulEffect : MonoBehaviour
{
    [Header("Settings(Ayarlar)")]
    public float lifeTime = 5f;
    public float soulGravity = 2f;
    public float stopThreshold = 0.5f;

    private Rigidbody2D rb;
    private bool timerStarted = false;
    private bool isSettled = false;
    private bool isTouchingGround = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Havada süzülmesin diye düşük direnç
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.5f;
    }

    void FixedUpdate()
    {
        if (!isSettled)
        {
            // Mermi gibi aşağı it
            rb.AddForce(Vector2.down * soulGravity, ForceMode2D.Impulse);

            // Yuvarlanması bittiyse (hızı kesildiyse) dondur
            if (isTouchingGround && rb.linearVelocity.magnitude < stopThreshold)
            {
                FreezeSoul();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGround(collision.gameObject))
        {
            isTouchingGround = true;

            if (!timerStarted)
            {
                timerStarted = true;
                Destroy(gameObject, lifeTime);
            }

            //  DÜZ ZEMİN RADARI: Çarptığımız yerin açısını ölçüyoruz
            if (collision.contactCount > 0)
            {
                // Çarptığımız noktanın "Normal" açısı (Yüzeyin nereye baktığı)
                Vector2 contactNormal = collision.GetContact(0).normal;

                // Eğer yüzeyin açısı düzse (y > 0.8 demek yukarı bakıyor, yani zemin düz demektir)
                if (contactNormal.y > 0.8f)
                {
                    FreezeSoul(); // Dümdüz asfalta düştü, anında kilitle!
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Eğer ruh köşeden yuvarlanıp sonra düzlüğe çıkarsa anında dondurmak için:
        if (!isSettled && IsGround(collision.gameObject) && collision.contactCount > 0)
        {
            if (collision.GetContact(0).normal.y > 0.8f)
            {
                FreezeSoul();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGround(collision.gameObject))
        {
            isTouchingGround = false;
        }
    }

    private bool IsGround(GameObject obj)
    {
        return obj.CompareTag("Ground") || obj.layer == LayerMask.NameToLayer("Ground");
    }

    // Objeyi tam anlamıyla donduran ve dönmesini yasaklayan fonksiyon
    private void FreezeSoul()
    {
        if (isSettled) return;
        isSettled = true;

        // Hızı ve dönüşü tamamen sıfırla
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.linearDamping = 20f;
        rb.angularDamping = 20f;

        
      
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }
}