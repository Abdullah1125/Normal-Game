using UnityEngine;

/// <summary>
/// Gyro-based box controller with dual deadzones and a timeout fallback for touch.
/// (Çift ölü bölgeye ve dokunmatik için zaman aşımı yedeğine sahip jiroskop kontrolcüsü.)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GyroBoxController : MonoBehaviour, IResettable
{
    [Header("Tilt Settings (Eğme Ayarları)")]
    public float tiltSpeed = 45f;

    [Header("Deadzone Settings (Ölü Bölge Ayarları)")]
    public float initialDeadZone = 0.7f;
    public float movingDeadZone = 0.2f;

    [Header("Safety Fallback (Güvenlik Yedeği)")]
    [Tooltip("Dokunmatik modun devreye girmesi için geçmesi gereken süre (Saniye)")]
    public float touchFallbackTime = 90f; // 1.5 dakika

    [Header("Touch Settings (Dokunmatik Ayarları)")]
    public float dragSpeed = 15f; // Sürükleme hızı
    public float dragFriction = 5f;
    public float grabRadius = 2.5f; //Tutma yarıçapı

    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCam;

    private bool canUseTouch = false;
    private bool isFallbackActive = false;
    private float activeTimer = 0f;
    private Vector3 originalPos;

    /// <summary>
    /// Assigns initial values and checks hardware support.
    /// (Başlangıç değerlerini atar ve donanım desteğini kontrol eder.)
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;

        rb.mass = 100f;
        originalPos = transform.position;

        InitialTouchCheck();
    }

    /// <summary>
    /// Evaluates if the device naturally needs touch controls.
    /// (Cihazın doğal olarak dokunmatik kontrole ihtiyacı olup olmadığını değerlendirir.)
    /// </summary>
    private void InitialTouchCheck()
    {
        // Cihazda sensör yoksa veya Editördeysek, baştan itibaren dokunmatik açıktır.
        if (!SystemInfo.supportsAccelerometer || Application.isEditor)
        {
            canUseTouch = true;
            isFallbackActive = true;
        }
        else
        {
            canUseTouch = false;
            isFallbackActive = false;
        }
    }

    /// <summary>
    /// Registers to the level manager.
    /// (Seviye yöneticisine kayıt olur.)
    /// </summary>
    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    /// <summary>
    /// Unregisters from the level manager to prevent memory leaks.
    /// (Bellek sızıntısını önlemek için seviye yöneticisinden kaydı siler.)
    /// </summary>
    void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    /// <summary>
    /// Tracks time to enable touch fallback if the player is stuck.
    /// (Oyuncu takılı kalırsa dokunmatik yedeği açmak için zamanı takip eder.)
    /// </summary>
    void Update()
    {
        // 1. Güvenlik Yedeği (Zamanlayıcı)
        if (!isFallbackActive)
        {
            activeTimer += Time.deltaTime;

            if (activeTimer >= touchFallbackTime)
            {
                canUseTouch = true;
                isFallbackActive = true;
                Debug.LogWarning("Sensör zaman aşımı: Kutu için dokunmatik modu aktif edildi.");
            }
        }

        // 2. Özel Dokunmatik Kontrol
        if (canUseTouch)
        {
            HandleManualTouch();
        }
    }

    /// <summary>
    /// Processes accelerometer input with dynamic deadzones.
    /// (İvmeölçer girdisini dinamik ölü bölgelerle işler.)
    /// </summary>
    void FixedUpdate()
    {
        // Her halükarda Unity'nin kendi Damping'ini kapatıyoruz ki yerçekimi bozulmasın
        rb.linearDamping = 0f;

        if (SystemInfo.supportsAccelerometer && !Application.isEditor)
        {
            Vector2 tiltForce = new Vector2(Input.acceleration.x, Input.acceleration.y);
            float currentDeadZone = (rb.linearVelocity.magnitude < 0.1f) ? initialDeadZone : movingDeadZone;

            if (tiltForce.magnitude > currentDeadZone)
            {
                rb.AddForce(tiltForce * tiltSpeed * rb.mass);
            }
            else if (!isDragging) 
            {
                // SİHİR BURADA: Telefon düzse sadece yatay (X) hızı sıfırla. 
                // Y hızına (yerçekimine) asla dokunma! Paraşüt efekti engellendi.
                float smoothedX = Mathf.Lerp(rb.linearVelocity.x, 0f, Time.fixedDeltaTime * 15f);
                rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
            }
        }
        else if (!isDragging) 
        {
            // Jiroskop yoksa ve sürüklenmiyorsa standart sürtünme (sadece X ekseni)
            float smoothedX = Mathf.Lerp(rb.linearVelocity.x, 0f, Time.fixedDeltaTime * dragFriction);
            rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Resets the box to its original position.
    /// (Kutuyu orijinal pozisyonuna sıfırlar. Zamanlayıcı korunur!)
    /// </summary>
    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isDragging = false;

        // NOT: activeTimer = 0f ve InitialTouchCheck() buradan silindi.
        // Artık oyuncu ölse bile süre işlemeye devam edecek veya açıldıysa açık kalacak.
    }

    #region Button Interaction (Buton Etkileşimi ve Fren)

    /// <summary>
    /// Reduces velocity when entering a button trigger.
    /// (Butona girildiğinde hızı düşürür.)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity *= 0.2f;
        }
    }

    /// <summary>
    /// Applies friction while staying on a button.
    /// (Buton üzerinde kalırken sürtünme uygular.)
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
        }
    }

    #endregion

    #region Manual Drag (Genişletilmiş Dokunmatik Yarıçapı)

    /// <summary>
    /// Ekrandaki dokunmaları matematiksel mesafeye göre işler.
    /// </summary>
    private void HandleManualTouch()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            touchPos.z = 0f;

            if (Vector2.Distance(transform.position, touchPos) <= grabRadius)
            {
                isDragging = true;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                offset = transform.position - touchPos;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPos = touchPos + offset;
            targetPos.z = 0f;

            Vector2 direction = (targetPos - transform.position);
            direction = Vector2.ClampMagnitude(direction, 2.0f);

            rb.linearVelocity = direction * dragSpeed;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    /// <summary>
    /// Unity Editöründe seçiliyken tutma yarıçapını yeşil bir çember olarak çizer.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }

    #endregion
}