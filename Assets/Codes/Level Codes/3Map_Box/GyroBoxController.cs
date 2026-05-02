using UnityEngine;
using UnityEngine.EventSystems;
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
    public float touchFallbackTime = 90f; // 1.5 dakika

    [Header("Touch Settings (Dokunmatik Ayarları)")]
    public float dragSpeed = 15f;
    public float dragFriction = 5f;
    public float grabRadius = 2.5f;

    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCam;

    private bool canUseTouch = false;
    private bool isHardwareMissing = false;
    private float activeTimer = 0f;
    private Vector3 originalPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        rb.mass = 100f;
        originalPos = transform.position;

        InitialTouchCheck();
    }

    private void InitialTouchCheck()
    {
        if (!SystemInfo.supportsAccelerometer || Application.isEditor)
        {
            canUseTouch = true;
            isHardwareMissing = true;
        }
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    void Update()
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.canMove) { isDragging = false; return; }
        if (Time.timeScale == 0f) { isDragging = false; return; }
        if (IsPointerOverUI()) return;

        // 90 saniye sayacı: Sadece dokunmatiği açar,
        if (!isHardwareMissing && !canUseTouch)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= touchFallbackTime)
            {
                canUseTouch = true;
            }
        }
        if (Time.timeScale == 0f)
        {
            isDragging = false;
            return;
        }
        if (canUseTouch)
        {
            HandleManualTouch();
        }
    }

    void FixedUpdate()
    {
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
                float smoothedX = Mathf.Lerp(rb.linearVelocity.x, 0f, Time.fixedDeltaTime * 15f);
                rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
            }
        }
        else if (!isDragging)
        {
            float smoothedX = Mathf.Lerp(rb.linearVelocity.x, 0f, Time.fixedDeltaTime * dragFriction);
            rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
        }
    }

    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isDragging = false;
    }

    #region Button Interaction 
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

    #region Manual Drag 
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
    #endregion

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        // PC (Mouse) kontrolü
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        // Mobil (Dokunmatik) kontrolü
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;

        return false;
    }
}