using UnityEngine;

/// <summary>
/// Creates an infinite falling loop, ignores ground, controls fall speed, and boosts horizontal speed.
/// (Sonsuz düþüþ döngüsü yaratýr, zemini yoksayar, düþüþ hýzýný kontrol eder ve yatay hýzý artýrýr.)
/// </summary>
public class EndlessFallController : MonoBehaviour
{
    [Header("Movement Settings (Hareket Ayarlarý)")]
    public float boostedSpeed = 20f;

    [Header("Teleport Coordinates (Iþýnlanma Koordinatlarý)")]
    public float bottomY = -15f;
    public float topY = 15f;

    [Header("Fall Speed Settings (Düþüþ Hýzý Ayarlarý)")]
    public float maxFallSpeed = 20f;
    public bool forceConstantSpeed = false;

    [Header("Layer Settings (Katman Ayarlarý)")]
    public string playerLayerName = "Player";
    public string groundLayerName = "Ground";

    private Transform player;
    private Rigidbody2D playerRb;
    private int playerLayerID;
    private int groundLayerID;

    /// <summary>
    /// Subscribes to the player reset event when the object is enabled.
    /// (Obje aktifleþtiðinde oyuncu sýfýrlanma event'ine abone olur.)
    /// </summary>
    private void OnEnable()
    {
        PlayerController.OnPlayerReset += ApplySpeedBoost;
    }

    /// <summary>
    /// Unsubscribes from events, resets player speed, and restores normal collision.
    /// (Kapanýþta event aboneliðini iptal eder, oyuncu hýzýný sýfýrlar ve çarpýþmalarý açar.)
    /// </summary>
    private void OnDisable()
    {
        PlayerController.OnPlayerReset -= ApplySpeedBoost;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ResetSpeed();
        }

        if (playerLayerID != -1 && groundLayerID != -1)
        {
            // Level bittiðinde veya kapanýnca zemin çarpýþmalarýný geri aç
            Physics2D.IgnoreLayerCollision(playerLayerID, groundLayerID, false);
        }
    }

    /// <summary>
    /// Gets references, applies speed boost, and disables collision between Player and Ground.
    /// (Referanslarý alýr, hýzý uygular ve Oyuncu ile Zemin arasýndaki çarpýþmayý iptal eder.)
    /// </summary>
    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance.transform;
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        ApplySpeedBoost();

        playerLayerID = LayerMask.NameToLayer(playerLayerName);
        groundLayerID = LayerMask.NameToLayer(groundLayerName);

        if (playerLayerID != -1 && groundLayerID != -1)
        {
            // Oyuncu ve Zemin birbirinin içinden geçer
            Physics2D.IgnoreLayerCollision(playerLayerID, groundLayerID, true);
        }
    }

    /// <summary>
    /// Applies the custom speed boost to the player.
    /// (Oyuncuya özel hýz takviyesini uygular.)
    /// </summary>
    private void ApplySpeedBoost()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetCustomSpeed(boostedSpeed);
        }
    }

    /// <summary>
    /// Controls the falling velocity of the player to prevent infinite acceleration.
    /// (Sonsuz ivmelenmeyi önlemek için oyuncunun düþüþ hýzýný kontrol eder.)
    /// </summary>
    private void FixedUpdate()
    {
        if (playerRb == null) return;

        if (playerRb.linearVelocity.y < 0)
        {
            if (forceConstantSpeed)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, -maxFallSpeed);
            }
            else
            {
                if (playerRb.linearVelocity.y < -maxFallSpeed)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, -maxFallSpeed);
                }
            }
        }
    }

    /// <summary>
    /// Checks player's Y position every frame and teleports if necessary.
    /// (Her karede oyuncunun Y pozisyonunu kontrol eder ve gerekirse ýþýnlar.)
    /// </summary>
    private void LateUpdate()
    {
        if (player == null) return;

        if (player.position.y < bottomY)
        {
            player.position = new Vector3(player.position.x, topY, player.position.z);
        }
    }
}