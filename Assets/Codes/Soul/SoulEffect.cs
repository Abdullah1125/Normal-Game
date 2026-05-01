using UnityEngine;
using System.Collections;

public class SoulEffect : MonoBehaviour
{
    [Header("Settings(Ayarlar)")]
    public float lifeTime = 5f;
    public float soulGravity = 2f;
    public float stopThreshold = 0.5f;

    private Rigidbody2D rb;
    private bool isSettled = false;
    private bool isTouchingGround = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // OBJE HER HAVUZDAN Ã‡IKTIÄINDA Ã‡ALIÅIR
    void OnEnable()
    {
        // Hafızayı sıfırla
        isSettled = false;
        isTouchingGround = false;

        // Rigidbody'yi canlandır
        rb.constraints = RigidbodyConstraints2D.None;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.5f;

        // Yaşam süresi bitince kendini yok etmek yerine "havuza geri dön"
        StopAllCoroutines();
        StartCoroutine(ReturnToPoolAfterTime());
    }

    private IEnumerator ReturnToPoolAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false); // Depoya geri dön!
    }

    void FixedUpdate()
    {
        if (!isSettled)
        {
            rb.AddForce(Vector2.down * soulGravity, ForceMode2D.Impulse);

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

            if (collision.contactCount > 0)
            {
                Vector2 contactNormal = collision.GetContact(0).normal;
                if (contactNormal.y > 0.8f)
                {
                    FreezeSoul();
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
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
        return obj.CompareTag(Constants.TAG_GROUND) || obj.layer == LayerMask.NameToLayer(Constants.TAG_GROUND);
    }

    private void FreezeSoul()
    {
        if (isSettled) return;
        isSettled = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.linearDamping = 20f;
        rb.angularDamping = 20f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }
}
