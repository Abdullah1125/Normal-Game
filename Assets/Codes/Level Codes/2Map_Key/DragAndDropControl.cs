using UnityEngine;

/// <summary>
/// Handles dragging and dropping the player using mouse or touch.
/// (Fare veya dokunmatik kullanarak oyuncuyu sÃ¼rÃ¼kleyip bÄ±rakmayÄ± yÃ¶netir.)
/// </summary>
public class DragAndDropControl : MonoBehaviour, IResettable
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    [Header("Settings (Ayarlar)")]
    public float grabRadius = 1f;
    public LayerMask playerLayer;
    public string playerTag = Constants.TAG_PLAYER;

    [Header("Boundaries (SÄ±nÄ±rlar)")]
    public bool useBoundaries = true;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    // Ã–nbelleÄŸe alÄ±nmÄ±ÅŸ bileÅŸen referansÄ±
    private Rigidbody2D playerRb;

    /// <summary>
    /// Caches camera and player references.
    /// (Kamera ve oyuncu referanslarÄ±nÄ± Ã¶nbelleÄŸe alÄ±r.)
    /// </summary>
    private void Awake()
    {
        cam = Camera.main;

        cam = Camera.main;

        // EÄŸer bu script doÄŸrudan oyuncu Ã¼zerindeyse referansÄ± al
        playerRb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Registers to the level management system.
    /// (Seviye yÃ¶netim sistemine kayÄ±t olur.)
    /// </summary>
    private void Start()
    {
        // EÄŸer script baÅŸka bir objede ise oyuncuyu Singleton Ã¼zerinden bul
        if (playerRb == null && PlayerController.Instance != null)
        {
            playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    /// <summary>
    /// Unregisters from the system to prevent memory leaks.
    /// (Bellek sÄ±zÄ±ntÄ±sÄ±nÄ± Ã¶nlemek iÃ§in sistem kaydÄ±nÄ± siler.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    private void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Manages the drag logic without redundant GetComponents.
    /// (Gereksiz GetComponent aramalarÄ± yapmadan sÃ¼rÃ¼kleme mantÄ±ÄŸÄ±nÄ± yÃ¶netir.)
    /// </summary>
    private void HandleInput()
    {
        if (playerRb == null) return;

        // 1. Karakteri Yakalama
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapCircle(mousePos, grabRadius, playerLayer);

            // GetComponent artÄ±k burada deÄŸil, Awake/Start iÃ§inde yapÄ±ldÄ±.
            if (hitCollider != null && hitCollider.CompareTag(playerTag))
            {
                isDragging = true;
                playerRb.bodyType = RigidbodyType2D.Kinematic;
                offset = playerRb.transform.position - mousePos;
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        // 2. SÃ¼rÃ¼kleme 
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            float targetX = mousePos.x + offset.x;
            float targetY = mousePos.y + offset.y;

            if (useBoundaries)
            {
                targetX = Mathf.Clamp(targetX, minX, maxX);
                targetY = Mathf.Clamp(targetY, minY, maxY);
            }

            playerRb.transform.position = new Vector3(targetX, targetY, playerRb.transform.position.z);
        }

        // 3. BÄ±rakma
        if (Input.GetMouseButtonUp(0))
        {
            ReleasePlayer();
        }
    }

    /// <summary>
    /// Returns the player to a dynamic physics state.
    /// (Oyuncuyu dinamik fizik durumuna geri dÃ¶ndÃ¼rÃ¼r.)
    /// </summary>
    private void ReleasePlayer()
    {
        if (playerRb != null)
        {
            isDragging = false;
            playerRb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    /// <summary>
    /// Implementation of IResettable to clean up state on level reset.
    /// (Seviye sÄ±fÄ±rlandÄ±ÄŸÄ±nda durumu temizlemek iÃ§in IResettable uygulamasÄ±.)
    /// </summary>
    public void ResetMechanic()
    {
        ReleasePlayer();
        isDragging = false;
    }
}
