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
        if (!isFallbackActive)
        {
            activeTimer += Time.deltaTime;

            // Belirlenen süre (90 saniye) aşıldığında dokunmatik kilidini aç
            if (activeTimer >= touchFallbackTime)
            {
                canUseTouch = true;
                isFallbackActive = true;
                Debug.LogWarning("Sensör zaman aşımı: Kutu için dokunmatik sürükleme modu aktif edildi.");
            }
        }
    }

    /// <summary>
    /// Processes accelerometer input with dynamic deadzones.
    /// (İvmeölçer girdisini dinamik ölü bölgelerle işler.)
    /// </summary>
    void FixedUpdate()
    {
        // Dokunmatik mod yedek olarak açılsa bile, sensör verisi okunmaya devam eder.
        // Bu sayede oyuncu ister telefonu eğer, isterse parmağıyla sürükler.
        if (SystemInfo.supportsAccelerometer && !Application.isEditor)
        {
            Vector2 tiltForce = new Vector2(Input.acceleration.x, Input.acceleration.y);
            float currentDeadZone = (rb.linearVelocity.magnitude < 0.1f) ? initialDeadZone : movingDeadZone;

            if (tiltForce.magnitude > currentDeadZone)
            {
                rb.AddForce(tiltForce * tiltSpeed * rb.mass);
            }
            else if (!isDragging) // Sürüklenirken ivmeölçerin kutuyu durdurmaya çalışmasını engeller
            {
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
            }
        }
    }

    /// <summary>
    /// Resets the box to its original position and clears timers.
    /// (Kutuyu orijinal pozisyonuna sıfırlar ve sayaçları temizler.)
    /// </summary>
    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isDragging = false;

        activeTimer = 0f;
        InitialTouchCheck(); // Süreyi ve dokunmatik durumunu orijinal haline döndürür
    }

    #region Button Interaction (Buton Etkileşimi ve Fren)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity *= 0.2f;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
        }
    }

    #endregion

    #region Manuel Sürükleme (Sensör Yoksa veya Süre Dolarsa Çalışır)

    private void OnMouseDown()
    {
        if (!canUseTouch) return;
        isDragging = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - touchPos;
    }

    private void OnMouseDrag()
    {
        if (!canUseTouch || !isDragging) return;
        Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = touchPos + offset;
        targetPos.z = 0f;
        rb.MovePosition(targetPos);
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    #endregion
}