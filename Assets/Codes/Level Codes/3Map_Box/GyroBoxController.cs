using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Gyro-based box controller with visual effects, ground detection, and friction audio.
/// (Görsel efektlere, yer tespitine ve sürtünme sesine sahip jiroskopik kutu kontrolcüsü.)
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class GyroBoxController : MonoBehaviour, IResettable
{
    [Header("Visual & Audio Effects (Görsel ve İşitsel Efektler)")]
    public ParticleSystem dustParticles; // Kayma esnasında çıkacak toz
    public float movementThreshold = 0.2f; // Efektin/Sesin tetiklenmesi için gereken min hız
    private AudioSource _audioSource;

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
        _audioSource = GetComponent<AudioSource>();
        mainCam = Camera.main;
        rb.mass = 100f;
        originalPos = transform.position;

        // Ses başlangıç ayarları (Döngü ve 3D ses)
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;

        InitialHardwareCheck();
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // SFX Ayarlarına Bağlan (Oyuncu menüden sesi kısarsa kutu da kısılır)
        if (SoundManager.Instance != null && SoundManager.Instance.sfxSource != null)
        {
            _audioSource.outputAudioMixerGroup = SoundManager.Instance.sfxSource.outputAudioMixerGroup;
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

        UpdateEffectsAndSound();
    }

    /// <summary>
    /// Toggles dust particles and friction sound based on ground contact and movement velocity.
    /// (Yer teması ve hareket hızına göre toz partiküllerini ve sürtünme sesini açar/kapatır.)
    /// </summary>
    private void UpdateEffectsAndSound()
    {
        // Hareket ediyor mu ve yere değiyor mu?
        bool isMoving = rb.linearVelocity.magnitude > movementThreshold;
        bool shouldPlay = isMoving && _isTouchingGround;

        // İşitsel Efekt (Ses)
        if (shouldPlay)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.clip = SoundManager.GetThemeClip(SFXType.BoxSlide);
                if (_audioSource.clip != null) _audioSource.Play();
            }
            // Jiroskopik harekette hızlanmalara göre perdeyi (pitch) dinamik değiştirir
            _audioSource.pitch = Mathf.Clamp(rb.linearVelocity.magnitude * 0.15f, 0.75f, 1.2f);
        }
        else
        {
            if (_audioSource.isPlaying) _audioSource.Stop();
        }

        // Görsel Efekt (Toz)
        if (dustParticles != null)
        {
            if (shouldPlay && !dustParticles.isPlaying) dustParticles.Play();
            else if (!shouldPlay && dustParticles.isPlaying) dustParticles.Stop();
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
        // Sadece yer tag'ine sahip objelerde toz çıksın ve ses çalsın
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
        if (_audioSource != null) _audioSource.Stop();
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