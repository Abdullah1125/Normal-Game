using UnityEngine;

/// <summary>
/// A pressure plate that swaps sprites when a box is placed on it.
/// (Üzerine kutu konulduğunda görsel değiştiren basınç plakası.)
/// </summary>
public class BoxButton : MonoBehaviour, IResettable
{
    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;  // Basılmamış hali
    public Sprite pressedSprite; // Basılmış hali

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

        // Başlangıç sprite'ını ayarla
        if (normalSprite != null && _sr != null)
        {
            _sr.sprite = normalSprite;
        }
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
        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton++;
            if (!_isPressed) PressButton();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!gameObject.scene.isLoaded || !other.gameObject.activeInHierarchy) return;

        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton--;
            if (_objectsOnButton <= 0 && _isPressed) ReleaseButton();
        }
    }

    /// <summary>
    /// Swaps to pressed sprite and triggers effects.
    /// (Basılmış görsele geçer ve efektleri tetikler.)
    /// </summary>
    private void PressButton()
    {
        _isPressed = true;

        // Sprite değiştirme
        if (_sr != null && pressedSprite != null)
        {
            _sr.sprite = pressedSprite;
        }

        if (pressParticles != null)
        {
            pressParticles.Stop();
            pressParticles.Play();
        }

        GateController.Instance?.OpenGate();
    }

    /// <summary>
    /// Reverts to the normal sprite.
    /// (Normal görsele geri döner.)
    /// </summary>
    private void ReleaseButton()
    {
        _isPressed = false;

        // Eski haline dön
        if (_sr != null && normalSprite != null)
        {
            _sr.sprite = normalSprite;
        }

        GateController.Instance?.CloseGate();
    }

    public void ResetMechanic()
    {
        _objectsOnButton = 0;
        _isPressed = false;

        if (_sr != null && normalSprite != null)
        {
            _sr.sprite = normalSprite;
        }

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