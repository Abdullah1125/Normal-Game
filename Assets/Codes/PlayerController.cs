using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 10f;
    public float airAcceleration = 25f;

    [Header("Zıplama Ayarları")]
    public float firstJumpForce = 13f;  
    public float doubleJumpForce = 10f;  
    public float fallMultiplier = 10f;
    public int extraJumpsValue = 1;
    private int extraJumps;

    [Header("Yumuşak Zirve Desteği")]
    public float extraBoostAmount = 2f;
    public float boostSmoothness = 3f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isHoldingJump;

    [Header("Sensörler")]
    public Transform groundCheck;
    public float checkRadius = 0.25f;
    public LayerMask groundLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 6f;
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        moveInput = horizontalInput;

        if (Input.GetButtonDown("Jump")) StartJump();
        if (Input.GetButtonUp("Jump")) StopJump();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) extraJumps = extraJumpsValue;

        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    void FixedUpdate()
    {
       
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

        
        if (isHoldingJump && rb.linearVelocity.y > 0.1f && rb.linearVelocity.y < 3f)
        {
            float targetY = rb.linearVelocity.y + extraBoostAmount;
            float smoothY = Mathf.Lerp(rb.linearVelocity.y, targetY, Time.fixedDeltaTime * boostSmoothness);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, smoothY);
        }

        
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    public void Move(float dir) => moveInput = dir;

    public void StartJump()
    {
        if (isGrounded)
        {
            ApplyJump(firstJumpForce);
        }
        else if (extraJumps > 0)
        {
            ApplyJump(doubleJumpForce);
            extraJumps--;
        }
        isHoldingJump = true;
    }

    public void StopJump() => isHoldingJump = false;

    private void ApplyJump(float force)
    {
       
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, force);
    }
}