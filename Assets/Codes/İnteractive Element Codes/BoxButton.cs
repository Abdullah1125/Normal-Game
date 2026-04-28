using UnityEngine;

/// <summary>
/// Buton tetikleyicisini yönetir. Kutu nesneleri (Box veya GyroBox) değdiğinde kapıyı açar.
/// </summary>
public class BoxButton : MonoBehaviour , IResettable
{
    [Header("Settings (Ayarlar)")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.green;

    [Header("Friction Settings (Sürtünme Ayarları)")]
    public float buttonFriction = 7f;

    private SpriteRenderer sr;
    private bool isPressed = false;

    private Rigidbody2D currentBoxRb;
    /// <summary>
    /// Bileşenleri başlatır ve varsayılan rengi atar.
    /// </summary>
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = normalColor;
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
    /// Kutu butona ilk değdiğinde tetiklenir.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsBox(other) && !isPressed)
        {
            PressButton();
        }
    }

    /// <summary>
    /// Kutu butonun üzerinde kaldığı sürece tetiklenir ve hafif sürtünme uygular.
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsBox(other))
        {
            if (!isPressed) PressButton();

            // Eğer rigidbody henüz hafızada yoksa veya farklı bir kutu geldiyse bir kere bul
            if (currentBoxRb == null || currentBoxRb.gameObject != other.gameObject)
            {
                currentBoxRb = other.GetComponent<Rigidbody2D>();
            }

            if (currentBoxRb != null)
            {
                currentBoxRb.linearVelocity -= currentBoxRb.linearVelocity * (buttonFriction * Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// Kutu butonun üzerinden ayrıldığında tetiklenir.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {

        if (!gameObject.scene.isLoaded) return;

        if (IsBox(other) && isPressed)
        {
            ReleaseButton();
        }
    }

    /// <summary>
    /// Temas eden objenin geçerli bir kutu olup olmadığını kontrol eder.
    /// </summary>
    private bool IsBox(Collider2D other)
    {
        // Prefab ne olursa olsun, Tag'i "Box" ise tamamdır!
        return other.CompareTag("Box");
    }

    /// <summary>
    /// Butonu aktif hale getirir ve kapıyı açar.
    /// </summary>
    private void PressButton()
    {
        isPressed = true;
        if (sr != null) sr.color = pressedColor;
        GateController.Instance?.OpenGate();
        Debug.Log("Buton aktif!");
    }

    private void ReleaseButton()
    {
        isPressed = false;
        currentBoxRb = null; // Hafızayı boşalt
        if (sr != null) sr.color = normalColor;
        GateController.Instance?.CloseGate(); // Kapıyı kapatmayı unutma!
    }

    public void ResetMechanic()
    {
        isPressed = false;
        currentBoxRb = null; // Hafızayı boşalt
        if (sr != null) sr.color = normalColor;
        gameObject.SetActive(true);
    }
    private void OnDestroy()
    {
        // Obje silinirken LevelManager'ın listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // Eğer LevelManager'da RemoveResettable fonksiyonu yoksa aşağıya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}