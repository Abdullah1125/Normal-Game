using UnityEngine;

/// <summary>
/// A pressure plate that swaps sprites and plays the same sound as GateButton when activated.
/// (Aktif olduğunda görsel değiştiren ve GateButton ile aynı sesi çalan basınç plakası.)
/// </summary>
public class BoxButton : MonoBehaviour, IResettable
{
    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem pressParticles;

    [Header("Physics Settings (Fizik Ayarları)")]
    public float buttonFriction = 7f;

    private SpriteRenderer _sr;
    private bool _isPressed = false;
    private int _objectsOnButton = 0;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (normalSprite != null && _sr != null) _sr.sprite = normalSprite;
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- BURASI KRİTİK: Level inaktifse buton hiçbir şeyi tetiklemesin ---
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            if (!LevelManager.Instance.activeLevel.isActive) return;
        }

        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton++;
            if (!_isPressed) PressButton();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Level inaktif olsa bile çıkışta "Release" tetiklenebilir mi? 
        // İstersen buraya da aynı kontrolü ekleyebilirsin ama genelde sadece giriş engellemek yeterlidir.
        if (!gameObject.scene.isLoaded || !other.gameObject.activeInHierarchy) return;

        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton--;
            if (_objectsOnButton <= 0 && _isPressed) ReleaseButton();
        }
    }

    private void PressButton()
    {
        _isPressed = true;

        if (_sr != null && pressedSprite != null) _sr.sprite = pressedSprite;

        if (SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Button);
        }

        if (pressParticles != null)
        {
            pressParticles.Stop();
            pressParticles.Play();
        }

        // GateController'ı tetikle
        GateController.Instance?.OpenGate();
    }

    private void ReleaseButton()
    {
        _isPressed = false;
        if (_sr != null && normalSprite != null) _sr.sprite = normalSprite;

        GateController.Instance?.CloseGate();
    }

    public void ResetMechanic()
    {
        _objectsOnButton = 0;
        _isPressed = false;
        if (_sr != null && normalSprite != null) _sr.sprite = normalSprite;
        if (pressParticles != null) pressParticles.Stop();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}