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

        // Baþlangýçta Sprite Renderer'daki görseli hafýzaya al
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
        return other.CompareTag("Player");
    }

    private void TryPressButton()
    {
        // ARADIÐIN KISIM BURASI: Level aktif deðilse buton basýlmaz, rengi kararýr
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

        // Sprite Deðiþimi
        if (sr != null && pressedSprite != null)
        {
            sr.sprite = pressedSprite;
            // Collider'ýn havada kalmasýný engellemek için güncelleme
            UpdateCollider();
        }

      

        // Ses Efekti
        if (SoundManager.instance != null)
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

        // Sprite'ý ve Rengi eski haline döndür
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
        // Sprite deðiþince karakterin havada kalmamasý için collider'ý yeniler
        if (polyCollider != null)
        {
            polyCollider.pathCount = 0;
        }
    }
    private void OnDestroy()
    {
        // Obje silinirken LevelManager'ýn listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // Eðer LevelManager'da RemoveResettable fonksiyonu yoksa aþaðýya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}