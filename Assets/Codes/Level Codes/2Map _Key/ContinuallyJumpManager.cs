using UnityEngine;

public class TiringJumpRule : MonoBehaviour
{
    public static TiringJumpRule Instance { get; private set; }

    [Header("Jump Settings (Zıplama Ayarları)")]
    public float startingForce = 15f;
    public float fatigueAmount = 1.5f;
    public float minimumForce = 4f;

    [Header("Fall Support (Zıplama Desteği)")]
    public float fallBoostMultiplier = 2.5f;
    public float maxForce = 20f;
    public float minFallDistance = 0.8f;

    [Header("Auto Jump (Otomatik Zıplama)")]
    public float jumpCooldown = 0.1f;

    private float currentForce;
    private float lastGroundY;
    private float lastJumpTime;
    private bool wasGrounded = false;

    private Rigidbody2D playerRb;
    private PlayerController player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        currentForce = startingForce;
    }

    /// <summary>
    /// Subscribe to the level start event.
    /// (Bölüm başlama olayına abone olur.)
    /// </summary>
    private void OnEnable()
    {
        // LevelManager her ResetAllMechanics dediğinde burası otomatik tetiklenir.
        LevelManager.OnLevelStarted += ResetJumpForce;
    }

    /// <summary>
    /// Unsubscribe to prevent memory leaks.
    /// (Bellek sızıntısını önlemek için abonelikten çıkar.)
    /// </summary>
    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ResetJumpForce;
    }

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            player = playerObj.GetComponent<PlayerController>();
            lastGroundY = playerObj.transform.position.y;
        }
    }

    void Update()
    {
        if (playerRb == null || player == null)
        {
            FindPlayer();
            return;
        }

        if (player.isGrounded && !wasGrounded)
        {
            OnLanded();
        }

        if (player.isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            AutoJump();
        }

        wasGrounded = player.isGrounded;
    }

    void OnLanded()
    {
        float currentY = player.transform.position.y;
        float heightDifference = lastGroundY - currentY;

        if (heightDifference > minFallDistance)
        {
            float gainedForce = heightDifference * fallBoostMultiplier;
            currentForce += gainedForce;
        }
        else
        {
            currentForce -= fatigueAmount;
        }

        currentForce = Mathf.Clamp(currentForce, minimumForce, maxForce);
        lastGroundY = currentY;
    }

    void AutoJump()
    {
        lastJumpTime = Time.time;
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.AddForce(Vector2.up * currentForce, ForceMode2D.Impulse);

        if (SoundManager.instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Jump);
        }
    }

    /// <summary>
    /// Resets the mechanics. Automatically called via LevelManager.OnLevelStarted.
    /// (Mekaniği sıfırlar. LevelManager.OnLevelStarted aracılığıyla otomatik çağrılır.)
    /// </summary>
    public void ResetJumpForce()
    {
        currentForce = startingForce;

        if (player != null)
        {
            lastGroundY = player.transform.position.y;
            wasGrounded = true;
        }
    }
}