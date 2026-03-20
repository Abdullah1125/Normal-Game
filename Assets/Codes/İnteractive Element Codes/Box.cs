using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Box : MonoBehaviour
{
    [Header("Sürtünme Ayarlarý")]
    public float slidingDamping = 0.5f;   // Sürüklenirkenki sürtünme (Düþük = Daha kaygan)
    public float stoppingDamping = 3.0f;  // Býrakýldýðýnda durma direnci (Yüksek = Hýzlý durur)
    public float stopThreshold = 0.1f;    // Hýz bu deðerin altýna düþerse zýnk diye durdur

    private Rigidbody2D rb;
    private bool isBeingPushed = false;

    public static Box Instance;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Profesyonel Rigidbody Ayarlarý
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Titremeyi önler
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (Instance == null) Instance = this;

    }

    void FixedUpdate()
    {
        // Eðer karakter itmiyorsa ve kutu hala hareket ediyorsa sürtünmeyi artýr
        if (!isBeingPushed && rb.linearVelocity.magnitude > stopThreshold)
        {
            // Zamanla sürtünmeyi artýrarak yumuþak duruþ saðla
            rb.linearDamping = Mathf.Lerp(rb.linearDamping, stoppingDamping, Time.fixedDeltaTime * 2f);
        }
        else if (!isBeingPushed && rb.linearVelocity.magnitude <= stopThreshold)
        {
            // Çok yavaþladýðýnda tamamen durdur ki sonsuza kadar kaymasýn
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isBeingPushed = true;
            rb.linearDamping = slidingDamping; // Ýterken yað gibi kaysýn
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isBeingPushed = false;
            // Býrakýldýðý an sürtünme kademeli artmaya baþlar (FixedUpdate içinde)
        }
    }
    
}