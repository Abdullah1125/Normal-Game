using UnityEngine;

/// <summary>
/// Jiroskop tabanlı kutu kontrolcüsü. Sensör yoksa dokunmatik sürüklemeye geçer.
/// BoxButton ile temasında kendi fren sistemini çalıştırır.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GyroBoxController : MonoBehaviour , IResettable
{
    [Header("Tilt Settings (Eğme Ayarları)")]
    public float tiltSpeed = 45f;
    public float deadZone = 0.15f;

    private Rigidbody2D rb;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCam;
    private bool canUseTouch = false;
    private Vector3 originalPos;

    /// <summary>
    /// Başlangıç değerlerini atar ve donanım kontrolü yapar.
    /// </summary>
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;

        rb.mass = 100f;
        originalPos = transform.position;

        if (!SystemInfo.supportsAccelerometer || Application.isEditor)
        {
            canUseTouch = true;
        }
        else
        {
            canUseTouch = false;
        }
    }
    void Start()
    {
        // Register to LevelManager (LevelManager'a kendini kaydettir)
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }
    /// <summary>
    /// İvmeölçer girdisini işler ve Deadzone kontrolü yapar.
    /// </summary>
    void FixedUpdate()
    {
        if (!canUseTouch)
        {
            Vector2 tiltForce = new Vector2(Input.acceleration.x, Input.acceleration.y);

            if (tiltForce.magnitude > deadZone)
            {
                rb.AddForce(tiltForce * tiltSpeed * rb.mass);
            }
            else
            {
                // Telefon düzse veya ivme azsa kutuyu zınk diye durdur
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
            }
        }
    }

    public void ResetMechanic()
    {
        transform.position = originalPos;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isDragging = false;
    }

    #region Button Interaction (Buton Etkileşimi ve Fren)

    private void OnTriggerEnter2D(Collider2D other)
    {
       
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity *= 0.2f;
            Debug.Log("GyroBox butonu hissetti, anlık fren yaptı!");
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // JİLET: Saniyede onlarca kez çalışan bu yerde Tag kullanmak hayat kurtarır
        if (other.CompareTag(Constants.TAG_BOX_BUTTON))
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 15f);
        }
    }

    #endregion

    #region Manuel Sürükleme (Sadece Sensör Yoksa Çalışır)

    /// <summary>
    /// Dokunma başladığında sürükleme ofsetini hesaplar.
    /// </summary>
    private void OnMouseDown()
    {
        if (!canUseTouch) return;
        isDragging = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - touchPos;
    }

    /// <summary>
    /// Sürükleme sırasında objenin pozisyonunu günceller.
    /// </summary>
    private void OnMouseDrag()
    {
        if (!canUseTouch || !isDragging) return;
        Vector3 touchPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPos = touchPos + offset;
        targetPos.z = 0f;
        rb.MovePosition(targetPos);
    }

    /// <summary>
    /// Dokunma bittiğinde sürüklemeyi sonlandırır.
    /// </summary>
    private void OnMouseUp()
    {
        isDragging = false;
    }

    #endregion
}
