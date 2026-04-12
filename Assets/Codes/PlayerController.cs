using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private SpriteRenderer sr;

    [Header("Movement Settings")]
    public float moveSpeed = 14f;       // Yatay hareket hızı
    public float defaultSpeed = 14f;
    public float airAcceleration = 25f; // Havada hızlanma katsayısı
    public bool canMove = true;

    [Header("Jump Settings")]
    public float firstJumpForce = 13f;  // İlk zıplama gücü
    public float doubleJumpForce = 10f; // Çift zıplama gücü
    public float fallMultiplier = 10f;  // Düşüş hızı çarpanı
    public int extraJumpsValue = 1;     // Toplam çift zıplama hakkı
    private int extraJumps;             // Kalan zıplama hakkı

    [Header("Juicy Movement")]
    public float acceleration = 75f;      // Yerden kalkış hızı
    public float deceleration = 50f;      // Yerde durma hızı
    public float airDeceleration = 10f;    // Havada süzülme (Düşük olursa momentum korunur)

    [Header("Jump Boost")]
    public float extraBoostAmount = 2f; // Zirve noktasındaki ekstra itiş
    public float boostSmoothness = 3f;  // İtişin yumuşaklığı

    [Header("Mobile Jump Assist")]
    public float coyotoTime = 0.2f;
    public float jumpBufferTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private float moveInput;            // Hareket girdisi (-1, 0, 1)
    public bool isGrounded { get; private set; }            // Yerde mi kontrolü
    private bool isHoldingJump;         // Zıplama tuşuna basılı tutuluyor mu?
    private float gravityDir;

    [Header("Sensors")]
    public Transform groundCheck;       // Yer kontrol objesi
    public float checkRadius = 0.25f;   // Kontrol dairesi yarıçapı
    public LayerMask groundLayer;       // Yer olarak sayılacak katman

    [Header("Death Settings")]
    private Vector2 startPos;           // Başlangıç noktası
    public GameObject soulPrefab;       // Ölünce çıkacak ruh objesi

    [Header("Sounds")]
    public AudioSource walkSound;

    void Start() => startPos = transform.position;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;       // Karakterin devrilmesini engelle
        rb.gravityScale = 6f;           // Yerçekimi ağırlığı
        moveSpeed = defaultSpeed;
        UpdateGravityDirection();
    }
    
    void Update()
    {
       
        // Klavye girdilerini al
        float keyboardInput = Input.GetAxisRaw("Horizontal");


        if (keyboardInput != 0) moveInput = keyboardInput;
        else if (Input.GetButtonUp("Horizontal")) moveInput = 0;

        if (!canMove)
        {
            moveInput = 0;
            return;
        }

        //Coyoto Time Sayacı
        if (isGrounded)
        {
            coyoteTimeCounter = coyotoTime;
            extraJumps = extraJumpsValue;
        } 
        else
            coyoteTimeCounter -= Time.deltaTime;

        //Jump Buffer sayacı
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        //Zıplama kontrolü
        if (jumpBufferCounter > 0f)
        {
            if (LevelManager.Instance.activeLevel.isJumpForbidden) return;

            if (coyoteTimeCounter > 0f && extraJumps == extraJumpsValue)
            {
                ApplyJump(firstJumpForce);
                coyoteTimeCounter = 0f;
                jumpBufferCounter = 0f;
                isHoldingJump = true;
            }
            else if (extraJumps > 0)
            {
                ApplyJump(doubleJumpForce);
                extraJumps--;
                jumpBufferCounter = 0f;
                isHoldingJump = true;
            }

        }

        if (Input.GetButtonUp("Jump")) StopJump();

      
        //Karakteri Ters Çevir
        if (gravityDir > 0) transform.eulerAngles = new Vector3(0, 0, 180f);
        else transform.eulerAngles = Vector3.zero;

        if (Mathf.Abs(moveInput) > 0.1f && isGrounded)
        {
            if (!walkSound.isPlaying)
            {
              
                walkSound.pitch = Random.Range(0.8f, 1.2f);
                walkSound.Play();
            }
        }
        else
        {
            walkSound.Stop();
        }

        UpdateVisuals();
    }
    private void UpdateVisuals()
    {
        // 1. Karakterin Yüzünü Çevir (Flip)
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            // Yer çekimi yukarıysa (baş aşağıysak) flip mantığını ters işle
            if (moveInput > 0.1f) sr.flipX = (gravityDir > 0);
            else if (moveInput < -0.1f) sr.flipX = !(gravityDir > 0);
        }

        // 2. Karakterin Gövdesini Ters Çevir (Rotation)
        if (gravityDir > 0) transform.eulerAngles = new Vector3(0, 0, 180f);
        else transform.eulerAngles = Vector3.zero;
    }

    void FixedUpdate()
    {

        float targetSpeed = moveInput * moveSpeed;
        float accelRate;

        // Yatay hareket fiziği
        if (isGrounded)
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        }
        else
        {
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? airAcceleration : airDeceleration;
        }

        float speedDif = targetSpeed - rb.linearVelocity.x;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, 0.95f) * Mathf.Sign(speedDif);
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);

        // Yerde olma kontrolü ve zıplama hakkı yenileme
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) extraJumps = extraJumpsValue;

        // Karakterin "yükselme" hızını yer çekimine göre kontrol etmeliyiz
        float upwardVelocity = -gravityDir * rb.linearVelocity.y;
        // Zıplama desteği (Jump Buffer/Apex Boost)
        if (isHoldingJump && rb.linearVelocity.y > 0.1f && rb.linearVelocity.y < 3f)
        {
            float targetY = rb.linearVelocity.y + (extraBoostAmount * -gravityDir);
            float smoothY = Mathf.Lerp(rb.linearVelocity.y, targetY, Time.fixedDeltaTime * boostSmoothness);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, smoothY);
        }

        // Hızlı düşüş fiziği
        // Eğer karakter yer çekimi yönünde hızlanıyorsa (düşüyorsa) çarpanı uygula
        bool isFalling = (gravityDir < 0 && rb.linearVelocity.y < 0) || (gravityDir > 0 && rb.linearVelocity.y > 0);
        if (isFalling)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    // Ölüm tetikleyicileri
    private void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Obstacle")) Die(); }
    private void OnCollisionEnter2D(Collision2D other) { if (other.gameObject.CompareTag("Obstacle")) Die(); }

    public void Die()
    {
        if (!canMove) return;
       
        if (CameraRoomController.Instance != null)
        {
            CameraRoomController.Instance.ShakeCamera();
        }
        if (soulPrefab != null) Instantiate(soulPrefab, transform.position, Quaternion.Euler(0, 0, 90f));
        StartCoroutine(DeathRoutine());
        SoundManager.PlaySFX(SoundManager.instance.dieSound);
       
    }
    private IEnumerator DeathRoutine()
    {
        canMove = false;
        moveInput = 0;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.21f);

        ResetPosition();
        if (sr != null) sr.enabled = true;

        canMove = true;
    }

    public void ResetPosition()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        Physics2D.gravity = new Vector2(0, -9.81f); 
        UpdateGravityDirection();

        // Diğer sistemleri sıfırla
        CameraRoomController.Instance.ResetCamera();
        LevelManager.Instance.ResetAllMechanics();
        if (CameraRoomController.Instance != null) CameraRoomController.Instance.ResetCamera();

        ResetSpeed();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ResetAllMechanics();
            LevelManager.Instance.ApplyLevel(); 
        }
    }

    public void Move(float dir)
    {
        // Seviye kurallarına göre (sol/sağ yasak) hareketi engelle
        LevelData data = LevelManager.Instance.activeLevel;
        
        if (data != null)
        {
            if (data.isLeftForbidden && dir < 0) { moveInput = 0; return; }
            if (data.isRightForbidden && dir > 0) { moveInput = 0; return; }
        }
        moveInput = dir;
    }
    public void StopJump() => isHoldingJump = false;

    private void ApplyJump(float force)
    {
        // Yer çekimi aşağıyken (-1) yukarı (+) zıplatır.
        // Yer çekimi yukarıyken (1) aşağı (-) zıplatır.
        SoundManager.PlaySFX(SoundManager.instance.jumpSound);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(new Vector2(0f, -gravityDir * force), ForceMode2D.Impulse);
       
    }
    public void StartJump()
    {
        jumpBufferCounter = jumpBufferTime;
        isHoldingJump = true;
    }
    public void ResetSpeed()
    {
        moveSpeed = defaultSpeed;
    }
    public void UpdateGravityDirection()
    {
        gravityDir = Mathf.Sign(Physics2D.gravity.y);
    }

}