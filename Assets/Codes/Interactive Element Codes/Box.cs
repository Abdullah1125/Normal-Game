using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Box : MonoBehaviour , IResettable
{
    [Header("Friction Settings(Sürtünme Ayarları)")]
    public float slidingDamping = 0.2f;   // Yandan itilirkenki sürtünme
    public float stoppingDamping = 3.0f;  // Bırakıldığında durma direnci (Zınk diye durması için artırdım)
    public float stopThreshold = 0.1f;

    private Rigidbody2D rb;
    private bool isBeingPushed = false;
    private Vector2 originalPos;

    // Kekeleme (Stuttering) Koruması için zamanlayıcı
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
        // 1. ZAMANLAYICI KONTROLÜ (Mikro sekmelerde kutu aniden durmasın diye)
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

        // 2. FREN SİSTEMİ
        if (!isBeingPushed)
        {
            if (rb.linearVelocity.magnitude > stopThreshold)
            {
                // Kutuyu daha kararlı durdurmak için Lerp yerine direkt direnç uyguluyoruz
                rb.linearDamping = stoppingDamping;
            }
            else
            {
                // Tamamen durdur
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // Sadece X'i sıfırla, yerçekimini bozma!
                rb.linearDamping = stoppingDamping;
            }
        }
    }

    // Enter yerine Stay kullanıyoruz ki oyuncu iterken sürekli tetiklensin
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
        {
            // SİHİR BURADA: Temasın yönünü (Normal) buluyoruz
            // Eğer normal.x 0.5'ten büyükse, bu yandan bir çarpışmadır (İtme)
            // Eğer normal.y büyükse, oyuncu kutunun üstündedir veya altındadır.
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
        // Obje silinirken LevelManager'ın listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // Eğer LevelManager'da RemoveResettable fonksiyonu yoksa aşağıya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}

