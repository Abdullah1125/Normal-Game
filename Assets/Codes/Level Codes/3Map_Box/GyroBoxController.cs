using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gyro-based box controller with visual effects and ground detection.
/// (Görsel efektlere ve yer tespitine sahip jiroskopik kutu kontrolcüsü.)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GyroBoxController : MonoBehaviour, IResettable
{
    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem dustParticles; // Kayma esnasında çıkacak toz
    public float movementThreshold = 0.2f; // Efektin tetiklenmesi için gereken min hız

    [Header("Tilt Settings (Eğme Ayarları)")]
    public float tiltSpeed = 45f;

    [Header("Deadzone Settings (Ölü Bölge Ayarları)")]
    public float initialDeadZone = 0.7f;
    public float movingDeadZone = 0.2f;

    [Header("Safety Fallback (Güvenlik Yedeği)")]
    public float touchFallbackTime = 90f;

    [Header("Touch Settings (Dokunmatik Ayarları)")]
    public float dragSpeed = 15f;
    public float dragFriction = 5f;
    public float grabRadius = 2.5f;

    private Rigidbody2D rb;
    private bool isDragging = false;
    private bool _isTouchingGround = false; // Yer temas kontrolü
    private Vector3 offset;
    private Camera mainCam;

    private bool canUseTouch = false;
    private bool isHardwareMissing = false;
    private float activeTimer = 0f;
    private Vector3 originalPos;

    /// <summary>
    /// Caches components and performs hardware check.
    /// (Bileşenleri önbelleğe alır ve donanım kontrolü yapar.)
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        rb.mass = 100f;
        originalPos = transform.position;

        InitialHardwareCheck();
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || PauseManager.isAdLoading)
        {
            if (isDragging || rb.linearVelocity != Vector2.zero) StopAllMovement();
            return;
        }

        if (!isDragging && IsPointerOverUI()) return;

        UpdateFallbackTimer();

        if (canUseTouch) HandleManualTouch();
    }

    private void FixedUpdate()
    {
        rb.linearDamping = 0f;

        if (SystemInfo.supportsAccelerometer && !Application.isEditor)
        {
            ApplyGyroForce();
        }
        else if (!isDragging)
        {
            ApplyBraking(dragFriction);
        }

        UpdateDustEffect();
    }

    /// <summary>
    /// Toggles dust particles based on ground contact and movement velocity.
    /// (Yer teması ve hareket hızına göre toz partiküllerini açar/kapatır.)
    /// </summary>
    private void UpdateDustEffect()
    {
        if (dustParticles == null) return;

        // Hareket ediyor mu ve yere değiyor mu?
        bool isMoving = rb.linearVelocity.magnitude > movementThreshold;

        if (isMoving && _isTouchingGround)
        {
            if (!dustParticles.isPlaying) dustParticles.Play();
        }
        else
        {
            if (dustParticles.isPlaying) dustParticles.Stop();
        }
    }

    #region Physics & Movement (Fizik ve Hareket)
    private void ApplyGyroForce()
    {
        Vector2 tiltForce = new Vector2(Input.acceleration.x, Input.acceleration.y);
        float currentDeadZone = (rb.linearVelocity.magnitude < 0.1f) ? initialDeadZone : movingDeadZone;

        if (tiltForce.magnitude > currentDeadZone)
        {
            rb.AddForce(tiltForce * tiltSpeed * rb.mass);
        }
        else if (!isDragging)
        {
            ApplyBraking(15f);
        }
    }

    private void ApplyBraking(float friction)
    {
        float smoothedX = Mathf.Lerp(rb.linearVelocity.x, 0f, Time.fixedDeltaTime * friction);
        rb.linearVelocity = new Vector2(smoothedX, rb.linearVelocity.y);
    }
    #endregion

    #region Collision Logic (Çarpışma Mantığı)
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Sadece yer tag'ine sahip objelerde toz çıksın
        if (collision.gameObject.CompareTag(Constants.TAG_GROUND))
        {
            _isTouchingGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_GROUND))
        {
            _isTouchingGround = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity *= 0.2f;
        }
    }
    #endregion

    public void ResetMechanic()
    {
        transform.position = originalPos;
        _isTouchingGround = false;
        if (dustParticles != null) dustParticles.Stop();
        StopAllMovement();
    }

    private void StopAllMovement()
    {
        isDragging = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void InitialHardwareCheck()
    {
        if (!SystemInfo.supportsAccelerometer || Application.isEditor)
        {
            canUseTouch = true;
            isHardwareMissing = true;
        }
    }

    private void UpdateFallbackTimer()
    {
        if (!isHardwareMissing && !canUseTouch)
        {
            activeTimer += Time.deltaTime;
            if (activeTimer >= touchFallbackTime) canUseTouch = true;
        }
    }

    #region Touch Interaction (Dokunmatik Etkileşim)
    private void HandleManualTouch()
    {
        if (Input.GetMouseButtonDown(0)) TryStartDrag();
        else if (Input.GetMouseButton(0) && isDragging) UpdateDragPosition();
        else if (Input.GetMouseButtonUp(0)) isDragging = false;
    }

    private void TryStartDrag()
    {
        Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        touchPos.z = 0f;

        if (Vector2.Distance(transform.position, touchPos) <= grabRadius)
        {
            isDragging = true;
            rb.linearVelocity = Vector2.zero;
            offset = transform.position - touchPos;
        }
    }

    private void UpdateDragPosition()
    {
        Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = touchPos + offset;
        targetPos.z = 0f;

        Vector2 direction = (targetPos - transform.position);
        rb.linearVelocity = Vector2.ClampMagnitude(direction * dragSpeed, 50f);
    }
    #endregion

    private void OnDestroy()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterResettable(this);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject() ||
               (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId));
    }
}