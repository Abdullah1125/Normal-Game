using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A button that stays pressed if it successfully opens the gate. 
/// Otherwise, it acts like a spring (reverts on exit).
/// (Eğer kapıyı başarıyla açıyorsa basılı kalan, aksi halde yaylı gibi eski haline dönen buton.)
/// </summary>
public class GateButton : MonoBehaviour, IResettable
{
    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    private SpriteRenderer _sr;
    private PolygonCollider2D _polyCollider;
    private bool _isPressed = false;

    [Header("Effects (Efektler)")]
    public ParticleSystem pressParticles;

    [Header("Events (Olaylar)")]
    public UnityEvent OnButtonPressed;

    /// <summary>
    /// Initializes components and saves initial state.
    /// (Bileşenleri başlatır ve başlangıç durumunu kaydeder.)
    /// </summary>
    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _polyCollider = GetComponent<PolygonCollider2D>();

        if (normalSprite == null && _sr != null)
        {
            normalSprite = _sr.sprite;
        }
    }

    /// <summary>
    /// Registers to LevelManager for reset functionality.
    /// (Sıfırlama işlevselliği için LevelManager'a kaydolur.)
    /// </summary>
    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            PressButton();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            ReleaseButton();
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag(Constants.TAG_PLAYER);
    }

    /// <summary>
    /// Handles the press logic. Stays down permanently if the level is active.
    /// (Basılma mantığını yönetir. Bölüm aktifse kalıcı olarak basılı kalır.)
    /// </summary>
    private void PressButton()
    {
        // Kapıyı zaten açtıysa tekrar çalışma (Already pressed check)
        if (_isPressed && LevelManager.Instance != null && LevelManager.Instance.activeLevel.isActive) return;

        _isPressed = true;

        if (pressParticles != null) pressParticles.Play();
        if (SoundManager.Instance != null) SoundManager.PlayThemeSFX(SFXType.Button);

        if (_sr != null && pressedSprite != null)
        {
            _sr.sprite = pressedSprite;
            UpdateCollider();
        }

        // Sadece level aktifse kapıyı tetikle
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel.isActive)
        {
            if (GateController.Instance != null)
            {
                GateController.Instance.OpenGate();
            }
            OnButtonPressed?.Invoke();
        }
    }

    /// <summary>
    /// Reverts the visual only if the gate was NOT opened.
    /// (Sadece kapı açılmadıysa görseli eski haline döndürür.)
    /// </summary>
    private void ReleaseButton()
    {
        // Eğer level aktifse (kapı açıldıysa) geri çıkmasın, basılı kalsın
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel.isActive)
        {
            return;
        }

        // Eğer sadece troll butonsa oyuncu gidince eski haline dön
        _isPressed = false;
        if (_sr != null && normalSprite != null)
        {
            _sr.sprite = normalSprite;
            UpdateCollider();
        }
    }

    /// <summary>
    /// Resets the button to its initial state when player dies.
    /// (Karakter öldüğünde butonu başlangıç durumuna sıfırlar.)
    /// </summary>
    public void ResetMechanic()
    {
        _isPressed = false;
        if (_sr != null)
        {
            _sr.sprite = normalSprite;
            _sr.color = Color.white;
        }
        UpdateCollider();
        gameObject.SetActive(true);
    }

    private void UpdateCollider()
    {
        // Collider'ı sprite'a göre güncelle (Update collider to sprite)
        if (_polyCollider != null) _polyCollider.pathCount = 0;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}