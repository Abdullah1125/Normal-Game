using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : Singleton<PlayerController>
{

    private SpriteRenderer sr;

    [Header("Movement Settings (Hareket AyarlarÃ„Â±)")]
    public float moveSpeed = 14f;
    public float defaultSpeed = 14f;
    public float airAcceleration = 25f;
    public bool canMove = true;

    [Header("Jump Settings (ZÃ„Â±plama AyarlarÃ„Â±)")]
    public float firstJumpForce = 13f;
    public float doubleJumpForce = 10f;
    public float fallMultiplier = 10f;
    public int extraJumpsValue = 1;
    private int extraJumps;

    [Header("Juicy Movement (AkÃ„Â±cÃ„Â± Hareket)")]
    public float acceleration = 75f;
    public float deceleration = 50f;
    public float airDeceleration = 10f;

    [Header("Jump Boost (ZÃ„Â±plama DesteÃ„Å¸i)")]
    public float extraBoostAmount = 2f;
    public float boostSmoothness = 3f;

    [Header("Mobile Jump Assist (Mobil ZÃ„Â±plama YardÃ„Â±mÃ„Â±)")]
    public float coyotoTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [Header("Anti-Spam Settings (Spam AyarlarÃ„Â±)")]
    public float jumpCooldownTime = 0.05f;
    private float jumpCooldownCounter;

    private Rigidbody2D rb;
    private float moveInput;
    public bool isGrounded { get; private set; }
    private bool isHoldingJump;
    private float gravityDir;

    [Header("Sensors (SensÃƒÂ¶rler)")]
    public Transform groundCheck;
    public float checkRadius = 0.25f;
    public LayerMask groundLayer;

    [Header("Death Settings (Ãƒâ€“lÃƒÂ¼m AyarlarÃ„Â±)")]
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
    /// Oyun baÃ…Å¸ladÃ„Â±Ã„Å¸Ã„Â±nda baÃ…Å¸langÃ„Â±ÃƒÂ§ pozisyonunu kaydeder ve zamanlayÃ„Â±cÃ„Â±yÃ„Â± baÃ…Å¸latÃ„Â±r.
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
    /// Gerekli bileÃ…Å¸enleri alÃ„Â±r ve varsayÃ„Â±lan deÃ„Å¸erleri atar.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;
        rb.gravityScale = 6f;
        moveSpeed = defaultSpeed;
        UpdateGravityDirection();
    }

    /// <summary>
    /// Girdi kontrollerini ve zÃ„Â±plama mantÃ„Â±Ã„Å¸Ã„Â±nÃ„Â± her karede iÃ…Å¸ler.
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

        // YukarÃ„Â± yÃƒÂ¶nlÃƒÂ¼ hÃ„Â±zÃ„Â± hesapla (Triple jump bug'Ã„Â±nÃ„Â± engeller)
        float currentUpwardVelocity = -gravityDir * rb.linearVelocity.y;

        // Yerdeyken ve zÃ„Â±plamÃ„Â±yorken haklarÃ„Â± yenile
        if (isGrounded && currentUpwardVelocity <= 0.1f)
        {
            coyoteTimeCounter = coyotoTime;
            extraJumps = extraJumpsValue;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // ZÃ„Â±plama tamponu (Jump Buffer)
        if (Input.GetButtonDown("Jump")) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // ZÃ„Â±plama cooldown sayacÃ„Â±nÃ„Â± dÃƒÂ¼Ã…Å¸ÃƒÂ¼r
        if (jumpCooldownCounter > 0f) jumpCooldownCounter -= Time.deltaTime;

        // ZÃ„Â±plama iÃ…Å¸lemini tetikle
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

        // AdÃ„Â±m sesi efektini yÃƒÂ¶net
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
    /// Karakterin yÃƒÂ¶nÃƒÂ¼nÃƒÂ¼ ve yer ÃƒÂ§ekimine gÃƒÂ¶re rotasyonunu gÃƒÂ¼nceller.
    /// </summary>
    private void UpdateVisuals()
    {
        // YÃƒÂ¼zÃƒÂ¼nÃƒÂ¼ ÃƒÂ§evir
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            if (moveInput > 0.1f) sr.flipX = (gravityDir > 0);
            else if (moveInput < -0.1f) sr.flipX = !(gravityDir > 0);
        }

        // GÃƒÂ¶vdeyi ters ÃƒÂ§evir (Ters yerÃƒÂ§ekimi)
        if (gravityDir > 0) transform.eulerAngles = new Vector3(0, 0, 180f);
        else transform.eulerAngles = Vector3.zero;
    }

    /// <summary>
    /// Fizik tabanlÃ„Â± hareketleri ve yer kontrolÃƒÂ¼nÃƒÂ¼ hesaplar.
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

        // Zemin kontrolÃƒÂ¼
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // ZÃ„Â±plama desteÃ„Å¸i (Apex boost)
        if (isHoldingJump && rb.linearVelocity.y > 0.1f && rb.linearVelocity.y < 3f)
        {
            float targetY = rb.linearVelocity.y + (extraBoostAmount * -gravityDir);
            float smoothY = Mathf.Lerp(rb.linearVelocity.y, targetY, Time.fixedDeltaTime * boostSmoothness);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, smoothY);
        }

        // HÃ„Â±zlÃ„Â± dÃƒÂ¼Ã…Å¸ÃƒÂ¼Ã…Å¸ (Fast fall)
        bool isFalling = (gravityDir < 0 && rb.linearVelocity.y < 0) || (gravityDir > 0 && rb.linearVelocity.y > 0);
        if (isFalling)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag(Constants.TAG_OBSTACLE)) Die(); }
    private void OnCollisionEnter2D(Collision2D other) { if (other.gameObject.CompareTag(Constants.TAG_OBSTACLE)) Die(); }

    /// <summary>
    /// Karakter engele ÃƒÂ§arptÃ„Â±Ã„Å¸Ã„Â±nda ÃƒÂ¶lÃƒÂ¼m iÃ…Å¸lemlerini baÃ…Å¸latÃ„Â±r.
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
    /// Ãƒâ€“lÃƒÂ¼m animasyonunu ve gecikmesini yÃƒÂ¶netir.
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
    /// Karakterin pozisyonunu, fiziÃ„Å¸ini ve durumunu varsayÃ„Â±lana sÃ„Â±fÃ„Â±rlar.
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

      
        if (LevelManager.Instance != null) LevelManager.Instance.ResetAllMechanics();
    }

    /// <summary>
    /// Seviye kurallarÃ„Â±nÃ„Â± dikkate alarak yatay hareket yÃƒÂ¶nÃƒÂ¼nÃƒÂ¼ atar.
    /// </summary>
    public void Move(float dir)
    {
        // Zaman durmuÃ…Å¸sa veya hareket yasaksa girdi alma (Ghost input korumasÃ„Â±)
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
    /// ZÃ„Â±plama komutunu sonlandÃ„Â±rÃ„Â±r.
    /// </summary>
    public void StopJump() => isHoldingJump = false;

    /// <summary>
    /// Fiziksel zÃ„Â±plama gÃƒÂ¼cÃƒÂ¼nÃƒÂ¼ uygular ve sistemleri sÃ„Â±fÃ„Â±rlar.
    /// </summary>
    private void ApplyJump(float force)
    {
        SoundManager.PlayThemeSFX(SFXType.Jump, 0.8f);
        if (isGrounded && jumpParticles != null) jumpParticles.Play();

        // Anti-Spam mÃƒÂ¼hÃƒÂ¼rleri
        isGrounded = false;
        jumpCooldownCounter = jumpCooldownTime;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0f, -gravityDir * force), ForceMode2D.Impulse);
    }

    /// <summary>
    /// Mobil kontroller iÃƒÂ§in zÃ„Â±plama tamponunu baÃ…Å¸latÃ„Â±r.
    /// </summary>
    public void StartJump()
    {
        // Zaman durmuÃ…Å¸sa girdi alma (Ghost input korumasÃ„Â±)
        if (!canMove || Time.timeScale == 0f) return;

        jumpBufferCounter = jumpBufferTime;
        isHoldingJump = true;
    }

    /// <summary>
    /// HÃ„Â±zÃ„Â± varsayÃ„Â±lan deÃ„Å¸erine dÃƒÂ¶ndÃƒÂ¼rÃƒÂ¼r.
    /// </summary>
    public void ResetSpeed() => moveSpeed = defaultSpeed;

    /// <summary>
    /// Mevcut yer ÃƒÂ§ekimi yÃƒÂ¶nÃƒÂ¼nÃƒÂ¼ belirler.
    /// </summary>
    public void UpdateGravityDirection() => gravityDir = Mathf.Sign(Physics2D.gravity.y);

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null) ScoreManager.Instance.StopTimer();
    }
}

