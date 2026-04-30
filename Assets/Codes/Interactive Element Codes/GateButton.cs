using UnityEngine;
using UnityEngine.Events;
public class GateButton : MonoBehaviour , IResettable
{
    [Header("Sprites(Görseller)")]
   
    public Sprite normalSprite;
    public Sprite pressedSprite;

    [Header("Colors(Renkeler)")]
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f);

    private bool isPressed = false;
    private SpriteRenderer sr;
    private PolygonCollider2D polyCollider;

    [Header("Effects(Efektler)")]
    public ParticleSystem pressParticles;

    [Header("Events (Olaylar)")]
    public UnityEvent OnButtonPressed;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        polyCollider = GetComponent<PolygonCollider2D>();

        // Başlangıçta Sprite Renderer'daki görseli hafızaya al
        if (normalSprite == null && sr != null)
        {
            normalSprite = sr.sprite;
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other) && !isPressed)
        {
            TryPressButton();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (IsPlayer(other) && !isPressed)
        {
            TryPressButton();
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag(Constants.TAG_PLAYER);
    }

    private void TryPressButton()
    {
        // ARADIĞIN KISIM BURASI: Level aktif değilse buton basılmaz, rengi kararır
        if (LevelManager.Instance != null && !LevelManager.Instance.activeLevel.isActive)
        {
            if (sr != null) sr.color = disabledColor;
            return;
        }

        PressButton();
    }

    private void PressButton()
    {
        isPressed = true;

        if (pressParticles != null)
        {
            pressParticles.Play();
        }

        // Sprite Değişimi
        if (sr != null && pressedSprite != null)
        {
            sr.sprite = pressedSprite;
            // Collider'ın havada kalmasını engellemek için güncelleme
            UpdateCollider();
        }

      

        // Ses Efekti
        if (SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Button);
        }

        // KAPIYI AÇAN KISIM
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
        }

        OnButtonPressed?.Invoke();
    }

  
    public void ResetMechanic() 
    {
        isPressed = false;

        // Sprite'ı ve Rengi eski haline döndür
        if (sr != null)
        {
            sr.sprite = normalSprite;
            sr.color = Color.white; // Veya orijinal rengin
            UpdateCollider();
        }

        gameObject.SetActive(true);
    }

    private void UpdateCollider()
    {
        // Sprite değişince karakterin havada kalmaması için collider'ı yeniler
        if (polyCollider != null)
        {
            polyCollider.pathCount = 0;
        }
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

