using UnityEngine;

/// <summary>
/// Manages the button trigger logic. Opens a gate when boxes are placed on it.
/// (Buton tetikleyicisini yönetir. Kutu nesneleri (Box) değdiğinde kapıyı açar.)
/// </summary>
public class BoxButton : MonoBehaviour, IResettable
{
    [Header("Settings (Ayarlar)")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.green;

    [Header("Friction Settings (Sürtünme Ayarları)")]
    public float buttonFriction = 7f;

    private SpriteRenderer sr;
    private bool isPressed = false;

    // Güvenlik ve performans değişkenleri
    private int objectsOnButton = 0; // Butonun üzerindeki kutu sayacı

    /// <summary>
    /// Initializes components and sets the default color.
    /// (Bileşenleri başlatır ve varsayılan rengi atar.)
    /// </summary>
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = normalColor;
    }

    /// <summary>
    /// Registers to the LevelManager.
    /// (LevelManager'a kendini kaydettirir.)
    /// </summary>
    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    /// <summary>
    /// Unregisters from the LevelManager.
    /// (Obje silinirken hafıza sızıntısını önlemek için listeden çıkar.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    /// <summary>
    /// Triggered when a box first touches the button.
    /// (Kutu butona ilk değdiğinde tetiklenir.)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsBox(other))
        {
            objectsOnButton++;

            if (!isPressed)
            {
                PressButton();
            }
        }
    }

    /// <summary>
    /// Applies light friction while a box stays on the button.
    /// (Kutu butonun üzerinde kaldığı sürece tetiklenir ve hafif sürtünme uygular.)
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsBox(other))
        {
            // GetComponent yerine direkt attachedRigidbody kullanıyoruz.
            // Bu Unity'nin kendi içinde önbelleğe aldığı bir özelliktir ve performans harcamaz.
            Rigidbody2D rb = other.attachedRigidbody;

            if (rb != null)
            {
                rb.linearVelocity -= rb.linearVelocity * (buttonFriction * Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// Triggered when a box leaves the button.
    /// (Kutu butonun üzerinden ayrıldığında tetiklenir.)
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!gameObject.scene.isLoaded) return; // Sahne kapanırken sahte çıkışları engeller

        if (IsBox(other))
        {
            objectsOnButton--;

            // Eğer butonun üzerinde hiç kutu kalmadıysa ve buton basılıysa kapıyı kapat
            if (objectsOnButton <= 0 && isPressed)
            {
                objectsOnButton = 0; // Eksi değerlere inmesini garanti altına alıyoruz
                ReleaseButton();
            }
        }
    }

    /// <summary>
    /// Checks if the colliding object is a valid box.
    /// (Temas eden objenin geçerli bir kutu olup olmadığını kontrol eder.)
    /// </summary>
    private bool IsBox(Collider2D other)
    {
        return other.CompareTag(Constants.TAG_BOX);
    }

    /// <summary>
    /// Activates the button and opens the gate.
    /// (Butonu aktif hale getirir ve kapıyı açar.)
    /// </summary>
    private void PressButton()
    {
        isPressed = true;
        if (sr != null) sr.color = pressedColor;

        GateController.Instance?.OpenGate();
    }

    /// <summary>
    /// Deactivates the button and closes the gate.
    /// (Butonu deaktif hale getirir ve kapıyı kapatır.)
    /// </summary>
    private void ReleaseButton()
    {
        isPressed = false;

        if (sr != null) sr.color = normalColor;

        GateController.Instance?.CloseGate();
    }

    /// <summary>
    /// Required implementation for IResettable. Resets the button's physics and visuals.
    /// (IResettable arayüzü için zorunlu fonksiyon. Butonun fiziksel ve görsel durumunu sıfırlar.)
    /// </summary>
    public void ResetMechanic()
    {
        objectsOnButton = 0;
        isPressed = false;

        if (sr != null) sr.color = normalColor;
        gameObject.SetActive(true);

        // GateController kendi IResettable koduyla sıfırlanacağı için burada kapatmıyoruz
    }
}
