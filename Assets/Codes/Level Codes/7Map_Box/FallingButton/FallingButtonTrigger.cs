using UnityEngine;

/// <summary>
/// Kutunun geldiğinde 2. butonun yukarıdan düşmesini tetikleyen ilk buton.
/// IResettable arayüzü sayesinde karakter öldüğünde kendini sıfırlar.
/// Mobil platformlar için optimize edilmiştir.
/// </summary>
public class FallingButtonTrigger : MonoBehaviour, IResettable
{
    [Header("Settings")]
    [Tooltip("Tetiklendiğinde düşecek olan 2. buton referansı.")]
    public FallingBoxButton fallingButton; // Düşecek olan 2. buton referansı

    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;
    private SpriteRenderer _sr;

    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem pressParticles;

    private bool _isPressed = false;
    private int _objectsOnButton = 0;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (normalSprite != null && _sr != null) _sr.sprite = normalSprite;
    }

    private void Start()
    {
        // Karakter öldüğünde (Level sıfırlandığında) haberdar olmak için sisteme kayıt oluyoruz
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Level aktif değilse etkileşime girme
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
        // Sahne yüklenmemişse veya obje inaktifse çıkış işlemi yapma
        if (!gameObject.scene.isLoaded || !other.gameObject.activeInHierarchy) return;

        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton--;
            if (_objectsOnButton <= 0 && _isPressed) ReleaseButton();
        }
    }

    /// <summary>
    /// Butona basılma işlemlerini ve görsel değişikliklerini uygular.
    /// Ardından ikinci butonun düşmesini tetikler.
    /// </summary>
    private void PressButton()
    {
        _isPressed = true;

        if (_sr != null && pressedSprite != null) _sr.sprite = pressedSprite;

        // Kendi SoundManager'ından buton sesini çal
        if (SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Button);
        }

        if (pressParticles != null)
        {
            pressParticles.Stop();
            pressParticles.Play();
        }

        // 2. Butonu düşür
        if (fallingButton != null)
        {
            fallingButton.DropButton();
        }
    }

    /// <summary>
    /// Kutu üzerinden kalktığında görseli normal haline döndürür.
    /// </summary>
    private void ReleaseButton()
    {
        // Kutu butondan çıkınca sadece görseli eski haline getir
        _isPressed = false;
        if (_sr != null && normalSprite != null) _sr.sprite = normalSprite;
    }

    /// <summary>
    /// Karakter öldüğünde veya bölüm sıfırlandığında IResettable tarafından tetiklenir.
    /// </summary>
    public void ResetMechanic()
    {
        // Karakter öldüğünde her şeyi başa sar
        _objectsOnButton = 0;
        _isPressed = false;

        if (_sr != null && normalSprite != null) _sr.sprite = normalSprite;
        if (pressParticles != null) pressParticles.Stop();
    }

    private void OnDestroy()
    {
        // Obje yok olurken IResettable sisteminden kaydı sil
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}
