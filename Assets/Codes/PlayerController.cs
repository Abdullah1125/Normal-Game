using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;       // Yatay hareket hızı
    public float airAcceleration = 25f; // Havada hızlanma katsayısı

    [Header("Jump Settings")]
    public float firstJumpForce = 13f;  // İlk zıplama gücü
    public float doubleJumpForce = 10f; // Çift zıplama gücü
    public float fallMultiplier = 10f;  // Düşüş hızı çarpanı
    public int extraJumpsValue = 1;     // Toplam çift zıplama hakkı
    private int extraJumps;             // Kalan zıplama hakkı

    [Header("Jump Boost")]
    public float extraBoostAmount = 2f; // Zirve noktasındaki ekstra itiş
    public float boostSmoothness = 3f;  // İtişin yumuşaklığı

    [Header("Mobile Jump Assist")]
    public float coyotoTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private float moveInput;            // Hareket girdisi (-1, 0, 1)
    private bool isGrounded;            // Yerde mi kontrolü
    private bool isHoldingJump;         // Zıplama tuşuna basılı tutuluyor mu?
    private float gravityDir;

    [Header("Sensors")]
    public Transform groundCheck;       // Yer kontrol objesi
    public float checkRadius = 0.25f;   // Kontrol dairesi yarıçapı
    public LayerMask groundLayer;       // Yer olarak sayılacak katman


    [Header("Death Settings")]
    private Vector2 startPos;           // Başlangıç noktası
    public GameObject soulPrefab;       // Ölünce çıkacak ruh objesi

    void Start() => startPos = transform.position;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;       // Karakterin devrilmesini engelle
        rb.gravityScale = 6f;           // Yerçekimi ağırlığı

    }
    
    void Update()
    {
        // Yer çekimi yönünü belirle (-1 aşağı, 1 yukarı)
        gravityDir = Mathf.Sign(Physics2D.gravity.y);

        // Klavye girdilerini al
        float keyboardInput = Input.GetAxisRaw("Horizontal");
        if (keyboardInput != 0) moveInput = keyboardInput;
        else if (Input.GetButtonUp("Horizontal")) moveInput = 0;

       
       

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

        // Karakterin yüzünü hareket yönüne çevir
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        //Karakteri Ters Çevir
        if (gravityDir > 0) transform.eulerAngles = new Vector3(0, 0, 180f);
        else transform.eulerAngles = Vector3.zero;
    }

    void FixedUpdate()
    {
        // Yatay hareket fiziği
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            float targetSpeed = moveInput * moveSpeed;
            float speedDif = targetSpeed - rb.linearVelocity.x;
            rb.AddForce(speedDif * airAcceleration * Vector2.right, ForceMode2D.Force);
        }

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
        if (soulPrefab != null) Instantiate(soulPrefab, transform.position, Quaternion.Euler(0, 0, 90f));
        ResetPosition();
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ApplyLevel();
        }
    }

    public void ResetPosition()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;
        // Diğer sistemleri sıfırla
        CameraRoomController.Instance.ResetCamera();
        LevelManager.Instance.ResetAllMechanics();
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
       rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    rb.AddForce(new Vector2(0f, -gravityDir * force), ForceMode2D.Impulse);
    }
    public void StartJump()
    {
        jumpBufferCounter = jumpBufferTime;
        isHoldingJump = true;
    }
}