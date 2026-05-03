using UnityEngine;

/// <summary>
/// Pushable box with strict friction, micro-stutter protection, and integrated A/V effects.
/// (Sıkı sürtünme, mikro sekme koruması ve entegre görsel/işitsel efektlere sahip itilebilir kutu.)
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class Box : MonoBehaviour, IResettable
{
    [Header("Friction Settings(Sürtünme Ayarları)")]
    public float slidingDamping = 0.2f;   // Yandan itilirkenki sürtünme
    public float stoppingDamping = 3.0f;  // Bırakıldığında durma direnci (Zınk diye durması için artırdım)
    public float stopThreshold = 0.1f;

    [Header("Effects & Audio (Efekt ve Ses)")]
    public ParticleSystem dustParticles;
    private AudioSource _audioSource;
    private bool _isTouchingGround = false; // Efektlerin havada çalmasını engeller

    private Rigidbody2D rb;
    private bool isBeingPushed = false;
    private Vector2 originalPos;

    // Kekeleme (Stuttering) Koruması için zamanlayıcı
    private float pushTimeout = 0.1f;
    private float pushTimer = 0f;

    // Not: Eğer sahnede birden fazla kutu olacaksa bu satır (Singleton) hata verebilir. 
    // Şimdilik kodunu bozmamak için bırakıyorum.
    public static Box Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>(); // Ses bileşeni

        // AudioSource temel ayarları (Sürtünme için loop şart)
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1f;

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

        // SFX Ayarlarına Bağlan (Oyuncu menüden sesi kısarsa kutu da kısılır)
        if (SoundManager.Instance != null && SoundManager.Instance.sfxSource != null)
        {
            _audioSource.outputAudioMixerGroup = SoundManager.Instance.sfxSource.outputAudioMixerGroup;
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

        // 3. EFEKT VE SES KONTROLÜ
        UpdateEffectsAndSound();
    }

    /// <summary>
    /// Manages dust particles and looping friction sound.
    /// (Toz partiküllerini ve döngüsel sürtünme sesini yönetir.)
    /// </summary>
    private void UpdateEffectsAndSound()
    {
        bool isMoving = rb.linearVelocity.magnitude > stopThreshold;
        bool shouldPlay = isMoving && isBeingPushed && _isTouchingGround;

        // İşitsel Efekt (Ses)
        if (shouldPlay)
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.clip = SoundManager.GetThemeClip(SFXType.BoxSlide);
                if (_audioSource.clip != null) _audioSource.Play();
            }
            // Hızlandıkça sesi dinamik olarak inceltir
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

        // Yere değme kontrolü (Efektler için)
        if (collision.gameObject.CompareTag(Constants.TAG_GROUND))
        {
            _isTouchingGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Yerden koptuğunda efektleri kesmek için
        if (collision.gameObject.CompareTag(Constants.TAG_GROUND))
        {
            _isTouchingGround = false;
        }
    }

    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        isBeingPushed = false;
        pushTimer = 0f;
        rb.linearDamping = stoppingDamping;

        // Reset anında efektleri de temizle
        _isTouchingGround = false;
        if (_audioSource != null) _audioSource.Stop();
        if (dustParticles != null) dustParticles.Stop();
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