using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player movement via drag and drop mechanics with boundary and UI checks.
/// (Sınır ve arayüz kontrolleriyle oyuncunun sürükle-bırak hareketini yönetir.)
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
    public float dragSpeed = 25f;
    public float deadZone = 0.1f;

    [Header("Boundaries (Sınırlar)")]
    public bool useBoundaries = true;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4.5f;
    public float maxY = 4.5f;

    private Rigidbody2D playerRb;

    /// <summary>
    /// Caches essential references and components on awake.
    /// (Bileşen ve referansları Awake sırasında önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        cam = Camera.main;
        playerRb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Registers the object to the level management system.
    /// (Nesneyi seviye yönetim sistemine kaydeder.)
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
    /// Unregisters the object to prevent memory leaks.
    /// (Bellek sızıntılarını önlemek için kaydı siler.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    /// <summary>
    /// Processes frame-based input and cancellation logic.
    /// (Kare tabanlı girdi ve iptal mantığını işler.)
    /// </summary>
    private void Update()
    {
        // Duraklatma, reklam veya hareket kısıtlaması durumlarını kontrol et
        bool shouldCancel = PauseManager.isAdLoading ||
                            Time.timeScale == 0f ||
                            (PlayerController.Instance != null && !PlayerController.Instance.canMove);

        if (shouldCancel)
        {
            // İptal durumunda sürüklemeyi bırak ve hızı sıfırla
            if (isDragging || (playerRb != null && (playerRb.linearVelocity != Vector2.zero || playerRb.angularVelocity != 0f)))
            {
                isDragging = false;
                if (playerRb != null)
                {
                    playerRb.linearVelocity = Vector2.zero;
                    playerRb.angularVelocity = 0f;
                }
            }
            return;
        }

        // Arayüz etkileşimi sırasında sürüklemeyi engelle
        if (!isDragging && IsPointerOverUI()) return;

        if (playerRb == null) return;

        HandleInput();
    }

    /// <summary>
    /// Manages mouse and touch input detection.
    /// (Fare ve dokunmatik girdi algılamasını yönetir.)
    /// </summary>
    private void HandleInput()
    {
        // Karakteri yakalama mantığı
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapCircle(mousePos, grabRadius, playerLayer);

            if (hitCollider != null && hitCollider.CompareTag(playerTag))
            {
                isDragging = true;
                offset = playerRb.transform.position - mousePos;
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
        }

        // Sürüklemeyi sonlandırma
        if (Input.GetMouseButtonUp(0))
        {
            ReleasePlayer();
        }
    }

    /// <summary>
    /// Executes physics-based movement and boundary clamping.
    /// (Fizik tabanlı hareketi ve sınırlandırmayı yürütür.)
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
            float distance = direction.magnitude;

            // Ölü bölge kontrolü ile titremeyi önle
            if (distance < deadZone)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
            else
            {
                direction = Vector2.ClampMagnitude(direction, 3.0f);
                playerRb.linearVelocity = direction * dragSpeed;
            }
        }
    }

    /// <summary>
    /// Resets the player to an idle physical state.
    /// (Oyuncuyu durağan fiziksel duruma döndürür.)
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
    /// Resets the mechanic state during level resets.
    /// (Seviye sıfırlamalarında mekanik durumunu sıfırlar.)
    /// </summary>
    public void ResetMechanic()
    {
        ReleasePlayer();
    }

    /// <summary>
    /// Validates if the pointer is currently interacting with UI.
    /// (İmlecin arayüzle etkileşimde olup olmadığını doğrular.)
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        // PC ve Mobil için arayüz kontrolü
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return true;

        return false;
    }
}