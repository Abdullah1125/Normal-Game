using UnityEngine;

/// <summary>
/// A pushable box that emits particles ONLY when grounded and moving.
/// (Sadece yere değdiğinde ve hareket ettiğinde partikül yayan kutu.)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Box : MonoBehaviour, IResettable
{
    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem dustParticles;

    [Header("Friction Settings (Sürtünme Ayarları)")]
    public float slidingDamping = 0.2f;
    public float stoppingDamping = 3.0f;
    public float stopThreshold = 0.1f;

    private Rigidbody2D _rb;
    private bool _isBeingPushed = false;
    private bool _isTouchingGround = false; // Yere değme kontrolü
    private Vector2 _originalPos;
    private float _pushTimer = 0f;
    private float _pushTimeout = 0.1f;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalPos = transform.position;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    void FixedUpdate()
    {
        HandlePushLogic();
        UpdateDustEffect();
    }

    /// <summary>
    /// Checks movement, push state, and ground contact to toggle particles.
    /// (Hareketi, itilme durumunu ve yer temasını kontrol ederek partikülleri yönetir.)
    /// </summary>
    private void UpdateDustEffect()
    {
        if (dustParticles == null) return;

        bool isMoving = _rb.linearVelocity.magnitude > stopThreshold;

        // ŞART: Hareket edecek + İtilecek + Yere değecek
        if (isMoving && _isBeingPushed && _isTouchingGround)
        {
            if (!dustParticles.isPlaying) dustParticles.Play();
        }
        else
        {
            if (dustParticles.isPlaying) dustParticles.Stop();
        }
    }

    private void HandlePushLogic()
    {
        if (_pushTimer > 0)
        {
            _pushTimer -= Time.fixedDeltaTime;
            _isBeingPushed = true;
            _rb.linearDamping = slidingDamping;
        }
        else
        {
            _isBeingPushed = false;
            _rb.linearDamping = stoppingDamping;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Oyuncu itme kontrolü
        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
        {
            if (Mathf.Abs(collision.contacts[0].normal.x) > 0.5f)
            {
                _pushTimer = _pushTimeout;
            }
        }

        // Yer kontrolü: Tag "Ground" olmalı
        if (collision.gameObject.CompareTag(Constants.TAG_GROUND))
        {
            _isTouchingGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Yerden ayrıldığında efekti kesmek için
        if (collision.gameObject.CompareTag(Constants.TAG_GROUND))
        {
            _isTouchingGround = false;
        }
    }

    public void ResetMechanic()
    {
        transform.position = _originalPos;
        _rb.linearVelocity = Vector2.zero;
        _isTouchingGround = false;
        if (dustParticles != null) dustParticles.Stop();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}