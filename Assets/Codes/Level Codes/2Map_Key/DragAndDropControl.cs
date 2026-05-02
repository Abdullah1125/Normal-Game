using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles dragging and dropping the player using mouse or touch. Includes jitter prevention.
/// (Fare veya dokunmatik kullanarak oyuncuyu sürüklemeyi yönetir. Titreme önleyici içerir.)
/// </summary>
public class DragAndDropControl : MonoBehaviour, IResettable
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    [Header("Settings (Ayarlar)")]
    public float grabRadius = 1f;
    public LayerMask playerLayer;
    public string playerTag = Constants.TAG_PLAYER; // Constants sınıfın varsa hata vermez
    public float dragSpeed = 25f;
    public float deadZone = 0.1f; // YENİ: Titremeyi kesecek milimetrik ölü bölge

    [Header("Boundaries (Sınırlar)")]
    public bool useBoundaries = true;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    private Rigidbody2D playerRb;

    /// <summary>
    /// Caches camera and player references.
    /// (Kamera ve oyuncu referanslarını önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        cam = Camera.main;
        playerRb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Registers to the level management system.
    /// (Seviye yönetim sistemine kayıt olur.)
    /// </summary>
    private void Start()
    {
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
    /// (Bellek sızıntısını önlemek için sistem kaydını siler.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    /// <summary>
    /// Player input detection for grabbing and releasing.
    /// (Oyuncunun tutma ve bırakma girdilerini kontrol eder.)
    /// </summary>
    private void Update()
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.canMove) { ReleasePlayer(); return; }
        if (Time.timeScale == 0f) { ReleasePlayer(); return; }
        if (IsPointerOverUI()) return;

        if (playerRb == null) return;

        if (Time.timeScale == 0f)
        {
            ReleasePlayer();
            return;
        }

        // 1. Karakteri Yakalama
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapCircle(mousePos, grabRadius, playerLayer);

            if (hitCollider != null && hitCollider.CompareTag(playerTag))
            {
                isDragging = true;
                offset = playerRb.transform.position - mousePos;
                playerRb.linearVelocity = Vector2.zero;
            }
        }

        // 2. Bırakma
        if (Input.GetMouseButtonUp(0))
        {
            ReleasePlayer();
        }
    }

    /// <summary>
    /// Physical drag calculation with deadzone to prevent jitter.
    /// (Duvar çarpışmalarını koruyarak ve titremeyi önleyerek fiziksel sürükleme yapar.)
    /// </summary>
    private void FixedUpdate()
    {
        if (isDragging && Input.GetMouseButton(0) && playerRb != null)
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            float targetX = mousePos.x + offset.x;
            float targetY = mousePos.y + offset.y;

            if (useBoundaries)
            {
                targetX = Mathf.Clamp(targetX, minX, maxX);
                targetY = Mathf.Clamp(targetY, minY, maxY);
            }

            Vector2 targetPos = new Vector2(targetX, targetY);
            Vector2 direction = (targetPos - playerRb.position);

            // YENİ: Mesafe Ölçümü (Titreme Koruması)
            float distance = direction.magnitude;

            // Eğer parmağa/hedefe çok yaklaştıysa hızı sıfırla ki çırpınmasın
            if (distance < deadZone)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
            else
            {
                // Uzaktaysa ona doğru gitmeye devam et
                direction = Vector2.ClampMagnitude(direction, 3.0f);
                playerRb.linearVelocity = direction * dragSpeed;
            }
        }
    }

    /// <summary>
    /// Returns the player to a normal state.
    /// (Oyuncuyu normal durumuna geri döndürür.)
    /// </summary>
    private void ReleasePlayer()
    {
        if (playerRb != null)
        {
            isDragging = false;
            playerRb.linearVelocity = Vector2.zero;
        }
    }

    /// <summary>
    /// Implementation of IResettable to clean up state on level reset.
    /// (Seviye sıfırlandığında durumu temizlemek için IResettable uygulaması.)
    /// </summary>
    public void ResetMechanic()
    {
        ReleasePlayer();
    }
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        // PC (Mouse) kontrolü
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        // Mobil (Dokunmatik) kontrolü
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;

        return false;
    }
}