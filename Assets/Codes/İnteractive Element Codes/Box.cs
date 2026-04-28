using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Box : MonoBehaviour , IResettable
{
    [Header("Friction Settings(Sürtünme Ayarlarý)")]
    public float slidingDamping = 0.2f;   // Yandan itilirkenki sürtünme
    public float stoppingDamping = 3.0f;  // Býrakýldýðýnda durma direnci (Zýnk diye durmasý için artýrdým)
    public float stopThreshold = 0.1f;

    private Rigidbody2D rb;
    private bool isBeingPushed = false;
    private Vector2 originalPos;

    // Kekeleme (Stuttering) Korumasý için zamanlayýcý
    private float pushTimeout = 0.1f;
    private float pushTimer = 0f;

    public static Box Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        originalPos = transform.position;
    }
    void Start()
    {
        // Register to LevelManager (LevelManager'a kendini kaydettir)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }
    void FixedUpdate()
    {
        // 1. ZAMANLAYICI KONTROLÜ (Mikro sekmelerde kutu aniden durmasýn diye)
        if (pushTimer > 0)
        {
            pushTimer -= Time.fixedDeltaTime;
            isBeingPushed = true;
            rb.linearDamping = slidingDamping;
        }
        else
        {
            isBeingPushed = false;
        }

        // 2. FREN SÝSTEMÝ
        if (!isBeingPushed)
        {
            if (rb.linearVelocity.magnitude > stopThreshold)
            {
                // Kutuyu daha kararlý durdurmak için Lerp yerine direkt direnç uyguluyoruz
                rb.linearDamping = stoppingDamping;
            }
            else
            {
                // Tamamen durdur
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Sadece X'i sýfýrla, yerçekimini bozma!
                rb.linearDamping = stoppingDamping;
            }
        }
    }

    // Enter yerine Stay kullanýyoruz ki oyuncu iterken sürekli tetiklensin
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // SÝHÝR BURADA: Temasýn yönünü (Normal) buluyoruz
            // Eðer normal.x 0.5'ten büyükse, bu yandan bir çarpýþmadýr (Ýtme)
            // Eðer normal.y büyükse, oyuncu kutunun üstündedir veya altýndadýr.
            float hitNormalX = Mathf.Abs(collision.contacts[0].normal.x);

            if (hitNormalX > 0.5f)
            {
                // Sadece yandan temas varsa itilme süresini yenile
                pushTimer = pushTimeout;
            }
        }
    }

    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        isBeingPushed = false;
        pushTimer = 0f;
        rb.linearDamping = stoppingDamping;
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