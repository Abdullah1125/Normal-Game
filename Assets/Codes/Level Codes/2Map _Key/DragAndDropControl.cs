using UnityEngine;

/// <summary>
/// Handles dragging and dropping the player using mouse or touch.
/// (Fare veya dokunmatik kullanarak oyuncuyu sürükleyip bırakmayı yönetir.)
/// </summary>
public class DragAndDropControl : MonoBehaviour, IResettable
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    [Header("Settings (Ayarlar)")]
    public float grabRadius = 1f;
    public LayerMask playerLayer;
    public string playerTag = "Player";

    [Header("Boundaries (Sınırlar)")]
    public bool useBoundaries = true;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    // Önbelleğe alınmış bileşen referansı
    private Rigidbody2D playerRb;

    /// <summary>
    /// Caches camera and player references.
    /// (Kamera ve oyuncu referanslarını önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        cam = Camera.main;

        // Eğer bu script doğrudan oyuncu üzerindeyse referansı al
        playerRb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Registers to the level management system.
    /// (Seviye yönetim sistemine kayıt olur.)
    /// </summary>
    private void Start()
    {
        // Eğer script başka bir objede ise oyuncuyu etiketiyle bul
        if (playerRb == null)
        {
            GameObject pObj = GameObject.FindWithTag(playerTag);
            if (pObj != null) playerRb = pObj.GetComponent<Rigidbody2D>();
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    /// <summary>
    /// Unregisters from the system to prevent memory leaks.
    /// (Bellek sızıntısını önlemek için sistem kaydını siler.)
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
    /// (Gereksiz GetComponent aramaları yapmadan sürükleme mantığını yönetir.)
    /// </summary>
    private void HandleInput()
    {
        if (playerRb == null) return;

        // 1. Karakteri Yakalama
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapCircle(mousePos, grabRadius, playerLayer);

            // GetComponent artık burada değil, Awake/Start içinde yapıldı.
            if (hitCollider != null && hitCollider.CompareTag(playerTag))
            {
                isDragging = true;
                playerRb.bodyType = RigidbodyType2D.Kinematic;
                offset = playerRb.transform.position - mousePos;
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        // 2. Sürükleme 
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

        // 3. Bırakma
        if (Input.GetMouseButtonUp(0))
        {
            ReleasePlayer();
        }
    }

    /// <summary>
    /// Returns the player to a dynamic physics state.
    /// (Oyuncuyu dinamik fizik durumuna geri döndürür.)
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
    /// (Seviye sıfırlandığında durumu temizlemek için IResettable uygulaması.)
    /// </summary>
    public void ResetMechanic()
    {
        ReleasePlayer();
        isDragging = false;
    }
}