using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private SpriteRenderer sr;

    [Header("Movement Settings (Hareket Ayarları)")]
    public float moveSpeed = 14f;
    public float defaultSpeed = 14f;
    public float airAcceleration = 25f;
    public bool canMove = true;

    [Header("Jump Settings (Zıplama Ayarları)")]
    public float firstJumpForce = 13f;
    public float doubleJumpForce = 10f;
    public float fallMultiplier = 10f;
    public int extraJumpsValue = 1;
    private int extraJumps;

    [Header("Juicy Movement (Akıcı Hareket)")]
    public float acceleration = 75f;
    public float deceleration = 50f;
    public float airDeceleration = 10f;

    [Header("Jump Boost (Zıplama Desteği)")]
    public float extraBoostAmount = 2f;
    public float boostSmoothness = 3f;

    [Header("Mobile Jump Assist (Mobil Zıplama Yardımı)")]
    public float coyotoTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [Header("Anti-Spam Settings (Spam Ayarları)")]
    public float jumpCooldownTime = 0.05f;
    private float jumpCooldownCounter;

    private Rigidbody2D rb;
    private float moveInput;
    public bool isGrounded { get; private set; }
    private bool isHoldingJump;
    private float gravityDir;

    [Header("Sensors (Sensörler)")]
    public Transform groundCheck;
    public float checkRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Death Settings (Ölüm Ayarları)")]
    private Vector2 startPos;
    public GameObject soulPrefab;

    [Header("Sounds (Sesler)")]
    public AudioSource walkSound;
    public float timeBetweenSteps = 0.35f;
    private float stepTimer;

    [Header("Particles (Efektler)")]
    public ParticleSystem jumpParticles;
    public ParticleSystem walkParticles;

    /// <summary>
    /// Oyun başladığında başlangıç pozisyonunu kaydeder ve zamanlayıcıyı başlatır.
    /// </summary>
    void Start()
    {
        startPos = transform.position;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StartTimer();
        }
    }

    /// <summary>
    /// Gerekli bileşenleri alır ve varsayılan değerleri atar.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
        rb.gravityScale = 6f;
        moveSpeed = defaultSpeed;
        UpdateGravityDirection();
    }

    /// <summary>
    /// Girdi kontrollerini ve zıplama mantığını her karede işler.
    /// </summary>
    void Update()
    {
        // Yatay klavye girdisini al
        float keyboardInput = Input.GetAxisRaw("Horizontal");

        if (keyboardInput != 0) moveInput = keyboardInput;
        else if (Input.GetButtonUp("Horizontal")) moveInput = 0;

        if (!canMove)
        {
            moveInput = 0;
            return;
        }

        // Yukarı yönlü hızı hesapla (Triple jump bug'ını engeller)
        float currentUpwardVelocity = -gravityDir * rb.linearVelocity.y;

        // Yerdeyken ve zıplamıyorken hakları yenile
        if (isGrounded && currentUpwardVelocity <= 0.1f)
        {
            coyoteTimeCounter = coyotoTime;
            extraJumps = extraJumpsValue;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Zıplama tamponu (Jump Buffer)
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // Zıplama cooldown sayacını düşür
        if (jumpCooldownCounter > 0f) jumpCooldownCounter -= Time.deltaTime;

        // Zıplama işlemini tetikle
        if (jumpBufferCounter > 0f && jumpCooldownCounter <= 0f)
        {
            if (LevelManager.Instance.activeLevel.isJumpForbidden) return;

            if (coyoteTimeCounter > 0f && extraJumps == extraJumpsValue)
            {
                ApplyJump(firstJumpForce);
                isHoldingJump = true;
            }
            else if (extraJumps > 0)
            {
                ApplyJump(doubleJumpForce);
                extraJumps--;
                isHoldingJump = true;
            }
        }

        if (Input.GetButtonUp("Jump")) StopJump();

        // Adım sesi efektini yönet
        if (Mathf.Abs(moveInput) > 0.1f && Mathf.Abs(rb.linearVelocity.x) > 1f && isGrounded && canMove)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                walkSound.pitch = Random.Range(0.85f, 1.15f);
                walkSound.PlayOneShot(walkSound.clip, 0.6f);

                if (walkParticles != null) walkParticles.Play();
                stepTimer = timeBetweenSteps;
            }
        }
        else if (stepTimer > 0)
        {
            stepTimer -= Time.deltaTime;
        }

        UpdateVisuals();
    }

    /// <summary>
    /// Karakterin yönünü ve yer çekimine göre rotasyonunu günceller.
    /// </summary>
    private void UpdateVisuals()
    {
        // Yüzünü çevir
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (moveInput > 0.1f) sr.flipX = (gravityDir > 0);
            else if (moveInput < -0.1f) sr.flipX = !(gravityDir > 0);
        }

        // Gövdeyi ters çevir (Ters yerçekimi)
        if (gravityDir > 0) transform.eulerAngles = new Vector3(0, 0, 180f);
        else transform.eulerAngles = Vector3.zero;
    }

    /// <summary>
    /// Fizik tabanlı hareketleri ve yer kontrolünü hesaplar.
    /// </summary>
    void FixedUpdate()
    {
        float targetSpeed = moveInput * moveSpeed;
        float accelRate = isGrounded ?
            (Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration) :
            (Mathf.Abs(targetSpeed) > 0.01f ? airAcceleration : airDeceleration);

        float speedDif = targetSpeed - rb.linearVelocity.x;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.95f) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        // Zemin kontrolü
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Zıplama desteği (Apex boost)
        if (isHoldingJump && rb.linearVelocity.y > 0.1f && rb.linearVelocity.y < 3f)
        {
            float targetY = rb.linearVelocity.y + (extraBoostAmount * -gravityDir);
            float smoothY = Mathf.Lerp(rb.linearVelocity.y, targetY, Time.fixedDeltaTime * boostSmoothness);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, smoothY);
        }

        // Hızlı düşüş (Fast fall)
        bool isFalling = (gravityDir < 0 && rb.linearVelocity.y < 0) || (gravityDir > 0 && rb.linearVelocity.y > 0);
        if (isFalling)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Obstacle")) Die(); }
    private void OnCollisionEnter2D(Collision2D other) { if (other.gameObject.CompareTag("Obstacle")) Die(); }

    /// <summary>
    /// Karakter engele çarptığında ölüm işlemlerini başlatır.
    /// </summary>
    public void Die()
    {
        if (!canMove) return;

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddDeath();
        if (CameraRoomController.Instance != null) CameraRoomController.Instance.ShakeCamera();

        GameObject soul = SoulPool.Instance.GetSoul();
        if (soul != null)
        {
            soul.transform.position = transform.position;
            soul.SetActive(true);
        }

        StartCoroutine(DeathRoutine());
        SoundManager.PlayThemeSFX(SFXType.Die);
    }

    /// <summary>
    /// Ölüm animasyonunu ve gecikmesini yönetir.
    /// </summary>
    private IEnumerator DeathRoutine()
    {
        canMove = false;
        moveInput = 0;

        if (sr != null) sr.enabled = false;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.32f);

        ResetPosition();
        if (sr != null) sr.enabled = true;
        canMove = true;
    }

    /// <summary>
    /// Karakterin pozisyonunu, fiziğini ve durumunu varsayılana sıfırlar.
    /// </summary>
    public void ResetPosition()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic;
        canMove = true;

        MobileDirectionButton.UpdateMovement();

        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        jumpCooldownCounter = 0f;

        ResetSpeed();

        if (CameraRoomController.Instance != null) CameraRoomController.Instance.ResetCamera();
        if (LevelManager.Instance != null) LevelManager.Instance.ResetAllMechanics();
    }

    /// <summary>
    /// Seviye kurallarını dikkate alarak yatay hareket yönünü atar.
    /// </summary>
    public void Move(float dir)
    {
        // Zaman durmuşsa veya hareket yasaksa girdi alma (Ghost input koruması)
        if (!canMove || Time.timeScale == 0f)
        {
            moveInput = 0;
            return;
        }

        LevelData data = LevelManager.Instance.activeLevel;
        if (data != null)
        {
            if (data.isLeftForbidden && dir < 0) { moveInput = 0; return; }
            if (data.isRightForbidden && dir > 0) { moveInput = 0; return; }
        }
        moveInput = dir;
    }

    /// <summary>
    /// Zıplama komutunu sonlandırır.
    /// </summary>
    public void StopJump() => isHoldingJump = false;

    /// <summary>
    /// Fiziksel zıplama gücünü uygular ve sistemleri sıfırlar.
    /// </summary>
    private void ApplyJump(float force)
    {
        SoundManager.PlayThemeSFX(SFXType.Jump, 0.8f);
        if (isGrounded && jumpParticles != null) jumpParticles.Play();

        // Anti-Spam mühürleri
        isGrounded = false;
        jumpCooldownCounter = jumpCooldownTime;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0f, -gravityDir * force), ForceMode2D.Impulse);
    }

    /// <summary>
    /// Mobil kontroller için zıplama tamponunu başlatır.
    /// </summary>
    public void StartJump()
    {
        // Zaman durmuşsa girdi alma (Ghost input koruması)
        if (!canMove || Time.timeScale == 0f) return;

        jumpBufferCounter = jumpBufferTime;
        isHoldingJump = true;
    }

    /// <summary>
    /// Hızı varsayılan değerine döndürür.
    /// </summary>
    public void ResetSpeed() => moveSpeed = defaultSpeed;

    /// <summary>
    /// Mevcut yer çekimi yönünü belirler.
    /// </summary>
    public void UpdateGravityDirection() => gravityDir = Mathf.Sign(Physics2D.gravity.y);

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.StopTimer();
    }
}