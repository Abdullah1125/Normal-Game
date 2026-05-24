using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A trigger-based button that toggles between normal and pressed sprites when jumped on.
/// Opens the gate after a required number of hits.
/// (Üzerine zýplandýðýnda görselleri arasýnda geçiþ yapan, isTrigger açýk kullanýlan buton.)
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class TriggerSpamButton : MonoBehaviour, IResettable
{
    [Header("Spam Settings (Spam Ayarlarý)")]
    public int requiredHits = 4;
    private int currentHits = 0;

    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    private SpriteRenderer _sr;
    private Collider2D _collider;
    private bool _isCrushed = false;

    // Mikro tetiklenmeleri önleyen kilit
    private bool _readyForNextHit = true;

    [Header("Effects (Efektler)")]
    public ParticleSystem pressParticles;

    [Header("Events (Olaylar)")]
    public UnityEvent OnButtonPressed;

    /// <summary>
    /// Initializes components and saves initial state.
    /// (Bileþenleri baþlatýr ve baþlangýç durumunu kaydeder.)
    /// </summary>
    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        if (normalSprite == null && _sr != null)
        {
            normalSprite = _sr.sprite;
        }
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Buton ezilmiþse veya bir sonraki vuruþ için henüz üstünden kalkýlmamýþsa pas geç
        if (_isCrushed || !_readyForNextHit) return;

        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Trigger sisteminde temas noktasý alamayýz, bu yüzden objelerin merkez noktalarýný kýyaslarýz.
            // Karakterin Y konumu, butonun Y konumundan yüksekteyse (yukarýdan geliyorsa) geçerli say.
            bool isPlayerAbove = other.transform.position.y > transform.position.y;

            if (isPlayerAbove)
            {
                RegisterHit();

                // Oyuncu trigger'a girdi, kilidi kapat.
                _readyForNextHit = false;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Oyuncu trigger alanýndan çýktý, kilidi aç.
            _readyForNextHit = true;

            // Oyun bitmediyse görseli tekrar normal haline döndür
            if (!_isCrushed && _sr != null && normalSprite != null)
            {
                _sr.sprite = normalSprite;
            }
        }
    }

    /// <summary>
    /// Registers a hit, changes the sprite to pressed, and plays particles.
    /// (Bir vuruþ kaydeder, görseli basýlý duruma getirir ve partikül oynatýr.)
    /// </summary>
    public void RegisterHit()
    {
        if (_isCrushed) return;

        currentHits++;

        if (_sr != null && pressedSprite != null)
        {
            _sr.sprite = pressedSprite;
        }

        if (pressParticles != null)
        {
            pressParticles.Stop();
            pressParticles.Play();
        }

        if (currentHits >= requiredHits)
        {
            CrushButton();
        }
    }

    /// <summary>
    /// Activates the fully crushed button permanently and opens the gate.
    /// (Tamamen ezilmiþ butonu kalýcý olarak aktifleþtirir ve kapýyý açar.)
    /// </summary>
    private void CrushButton()
    {
        _isCrushed = true;

        if (SoundManager.Instance != null) SoundManager.PlayThemeSFX(SFXType.Button);

        if (_sr != null && pressedSprite != null)
        {
            _sr.sprite = pressedSprite;
        }

        if (_collider is PolygonCollider2D poly) poly.pathCount = 0;

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
    /// Resets the button to its initial uncrushed state.
    /// (Butonu ezilmemiþ baþlangýç durumuna sýfýrlar.)
    /// </summary>
    public void ResetMechanic()
    {
        _isCrushed = false;
        currentHits = 0;
        _readyForNextHit = true;

        if (_sr != null)
        {
            _sr.sprite = normalSprite;
            _sr.color = Color.white;
        }

        if (_collider is PolygonCollider2D poly) poly.pathCount = 0;

        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}