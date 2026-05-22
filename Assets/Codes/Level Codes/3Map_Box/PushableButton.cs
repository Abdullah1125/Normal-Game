using UnityEngine;

/// <summary>
/// Unity 6 button using BoxCast for a thicker detection area.
/// Active logic: Forgiving mechanic (red zone permanently disappears after first touch).
/// Handles custom sprites for pressed and unpressed states.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PushableButton : MonoBehaviour, IResettable
{
    [Header("Detection Area Settings (Algılama Alanı Ayarları)")]
    public LayerMask boxLayer;
    public float rayDistance = 0.4f;
    public float rayWidth = 0.5f;
    public float originOffset = 0.1f;
    public float tipSensitivity = 0.05f;

    [Header("Physics Settings (Fizik Ayarları)")]
    public float slidingDamping = 0.5f;
    public float stoppingDamping = 3.0f;

    [Header("Sprite Settings (Görsel Ayarlar)")]
    public Sprite normalSprite; // Basılı olmayan halinin görseli
    public Sprite pressedSprite; // Basılı halinin görseli

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D myCollider;

    private bool isPressed = false;
    private bool hasActivatedOnce = false;
    private Vector2 originalPos;

    /// <summary>
    /// Bileşenleri alır, trigger durumunu kapatır, başlangıç pozisyonunu ve görselini kaydeder.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();

        if (myCollider != null) myCollider.isTrigger = false;

        originalPos = transform.position;

        // Eğer Inspector'dan normal görsel atanmadıysa, üzerindeki mevcut görseli baz al
        if (normalSprite == null && sr != null)
        {
            normalSprite = sr.sprite;
        }
    }

    /// <summary>
    /// LevelManager sistemine kendini sıfırlanabilir obje olarak kaydeder.
    /// </summary>
    void Start()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterResettable(this);
    }

    /// <summary>
    /// Fizik motoru güncellemelerinde BoxCast taramasını ve sürtünmeyi hesaplar.
    /// </summary>
    void FixedUpdate()
    {
        CheckThickRay();
        ApplyFriction();
    }

    /// <summary>
    /// Kalın ışın taraması yapar. İlk temasta kırmızı alanı iptal eder. 
    /// Duruma göre basılma veya bırakılma fonksiyonlarını çağırır.
    /// </summary>
    private void CheckThickRay()
    {
        Vector2 rayOrigin = new Vector2(myCollider.bounds.center.x, myCollider.bounds.min.y + originOffset);
        float totalRayLength = rayDistance + originOffset;
        Vector2 boxSize = new Vector2(rayWidth, 0.01f);

        RaycastHit2D hit = Physics2D.BoxCast(rayOrigin, boxSize, 0f, Vector2.down, totalRayLength, boxLayer);

        bool shouldActivate = false;

        // --- AKTİF MANTIK: AFFEDİCİ MEKANİK ---
        if (hit.collider != null && hit.collider.CompareTag("Box"))
        {
            if (hasActivatedOnce)
            {
                shouldActivate = true;
            }
            else if (hit.distance >= (totalRayLength - tipSensitivity))
            {
                hasActivatedOnce = true;
                shouldActivate = true;
            }
        }

        if (shouldActivate && !isPressed) Press();
        else if (!shouldActivate && isPressed) Release();
    }

    /// <summary>
    /// Unity 6 API (linearDamping) standartlarına göre sürtünme değerlerini uygular.
    /// </summary>
    private void ApplyFriction()
    {
        rb.linearDamping = rb.linearVelocity.magnitude < 0.1f ? stoppingDamping : slidingDamping;
    }

    /// <summary>
    /// Buton basıldı durumuna geçer, özel görselini yükler ve kapıyı açar.
    /// </summary>
    private void Press()
    {
        isPressed = true;
        if (sr != null && pressedSprite != null) sr.sprite = pressedSprite;
        GateController.Instance?.OpenGate();
    }

    /// <summary>
    /// Buton serbest durumuna geçer, normal görseline döner ve kapıyı kapatır.
    /// </summary>
    private void Release()
    {
        isPressed = false;
        if (sr != null && normalSprite != null) sr.sprite = normalSprite;
        GateController.Instance?.CloseGate();
    }

    /// <summary>
    /// Sistemi başlangıç pozisyonuna taşır ve tüm kilit ile görsel hafızasını sıfırlar.
    /// </summary>
    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        hasActivatedOnce = false;

        if (isPressed) Release();
    }

    /// <summary>
    /// Editörde kalın BoxCast alanını transparan renkli dikdörtgenler olarak çizer.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (myCollider == null) return;

        Vector3 startPoint = new Vector3(myCollider.bounds.center.x, myCollider.bounds.min.y + originOffset, 0);
        float totalLength = rayDistance + originOffset;
        Vector3 tipStart = startPoint + Vector3.down * (totalLength - tipSensitivity);

        float redZoneLength = totalLength - tipSensitivity;
        Vector3 redZoneCenter = startPoint + Vector3.down * (redZoneLength / 2f);
        Vector3 redZoneSize = new Vector3(rayWidth, redZoneLength, 0f);

        Vector3 greenZoneCenter = tipStart + Vector3.down * (tipSensitivity / 2f);
        Vector3 greenZoneSize = new Vector3(rayWidth, tipSensitivity, 0f);

        if (Application.isPlaying && hasActivatedOnce)
        {
            Vector3 fullCenter = startPoint + Vector3.down * (totalLength / 2f);
            Vector3 fullSize = new Vector3(rayWidth, totalLength, 0f);
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(fullCenter, fullSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(fullCenter, fullSize);
        }
        else
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawCube(redZoneCenter, redZoneSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(redZoneCenter, redZoneSize);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(greenZoneCenter, greenZoneSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(greenZoneCenter, greenZoneSize);
        }
    }
}