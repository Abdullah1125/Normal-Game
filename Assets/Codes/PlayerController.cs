using UnityEngine;
using UnityEngine.InputSystem;

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

    private Rigidbody2D rb;
    private float moveInput;            // Hareket girdisi (-1, 0, 1)
    private bool isGrounded;            // Yerde mi kontrolü
    private bool isHoldingJump;         // Zıplama tuşuna basılı tutuluyor mu?

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
        // Klavye girdilerini al
        float keyboardInput = Input.GetAxisRaw("Horizontal");
        if (keyboardInput != 0) moveInput = keyboardInput;
        else if (Input.GetButtonUp("Horizontal")) moveInput = 0;

        // Zıplama kontrolleri
        if (Input.GetButtonDown("Jump")) StartJump();
        if (Input.GetButtonUp("Jump")) StopJump();

        // Yerde olma kontrolü ve zıplama hakkı yenileme
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) extraJumps = extraJumpsValue;

        // Karakterin yüzünü hareket yönüne çevir
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
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

        // Zıplama desteği (Jump Buffer/Apex Boost)
        if (isHoldingJump && rb.linearVelocity.y > 0.1f && rb.linearVelocity.y < 3f)
        {
            float targetY = rb.linearVelocity.y + extraBoostAmount;
            float smoothY = Mathf.Lerp(rb.linearVelocity.y, targetY, Time.fixedDeltaTime * boostSmoothness);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, smoothY);
        }

        // Hızlı düşüş fiziği
        if (rb.linearVelocity.y < 0)
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
    }

    public void ResetPosition()
    {
        transform.position = startPos;
        rb.linearVelocity = Vector2.zero;

        // Diğer sistemleri sıfırla
        SecretAreaTrigger trigger = FindFirstObjectByType<SecretAreaTrigger>();
        if (trigger != null) trigger.ResetTrigger();

        FindFirstObjectByType<CameraRoomController>().ResetCamera();
        ResetLevelObjects();
    }

    private void ResetLevelObjects()
    {
        // Kapıları, anahtarları ve butonları bulup sıfırla
        var gates = Object.FindObjectsByType<GateController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var g in gates) g.ResetGate();

        var keys = Object.FindObjectsByType<Key>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var k in keys) k.ResetKey();

        var buttons = Object.FindObjectsByType<GateButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var b in buttons) b.ResetButton();
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

    public void StartJump()
    {
        // Zıplama yasağı kontrolü ve zıplama uygulaması
        if (LevelManager.Instance.activeLevel.isJumpForbidden) return;

        if (isGrounded) ApplyJump(firstJumpForce);
        else if (extraJumps > 0)
        {
            ApplyJump(doubleJumpForce);
            extraJumps--;
        }
        isHoldingJump = true;
    }

    public void StopJump() => isHoldingJump = false;

    private void ApplyJump(float force) => rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
}