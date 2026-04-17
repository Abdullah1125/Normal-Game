using UnityEngine;
using UnityEngine.Tilemaps;

public class TiringJumpRule : MonoBehaviour
{
    public static TiringJumpRule Instance { get; private set; }
   

    [Header("Jump Settings(Zıplama Ayarları)")]
    public float startingForce = 15f;
    public float fatigueAmount = 1.5f;
    public float minimumForce = 4f;

    [Header("Fall Support(Zıplama Desteği)")]
    public float fallBoostMultiplier = 2.5f;
    public float maxForce = 20f;
    public float minFallDistance = 0.8f;

    [Header("Auto Jump(Otomatik Zıplama)")]
    public float jumpCooldown = 0.1f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private float currentForce;
    private float lastGroundY;         
    private float lastJumpTime;
    private bool wasGrounded = false;

    private Rigidbody2D playerRb;
    private PlayerController player;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
       

       
            currentForce = startingForce;
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

            if (showDebugLogs)
                Debug.Log($"↑ Aşağı düştü: {heightDifference:F1}m | +{gainedForce:F1} güç | Toplam: {currentForce:F1}");
        }
        else
        {
         
            currentForce -= fatigueAmount;

            if (showDebugLogs)
                Debug.Log($"↓ Yorulma | -{fatigueAmount:F1} güç | Kalan: {currentForce:F1}");
        }

        
        currentForce = Mathf.Clamp(currentForce, minimumForce, maxForce);

       
        lastGroundY = currentY;
    }

    void AutoJump()
    {
        lastJumpTime = Time.time;

        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.AddForce(Vector2.up * currentForce, ForceMode2D.Impulse);

        if (showDebugLogs)
            Debug.Log($" Zıplama! Güç: {currentForce:F1}");
    }

    public void ResetJumpForce()
    {
        currentForce = startingForce;
        if (player != null) lastGroundY = player.transform.position.y;
    }

    public void SetActive(bool active)
    {
        enabled = active;
        if (active) ResetJumpForce();
    }
}