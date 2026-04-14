using UnityEngine;

public class GateButton : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Boþ býrakýrsanýz Sprite Renderer'daki mevcut görseli alýr.")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    [Header("Colors (Opsiyonel)")]
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f);

    private bool isPressed = false;
    private SpriteRenderer sr;
    private PolygonCollider2D polyCollider;

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

        // Sprite Deðiþimi
        if (sr != null && pressedSprite != null)
        {
            sr.sprite = pressedSprite;
            // Collider'ýn havada kalmasýný engellemek için güncelleme
            UpdateCollider();
        }

        // Yerçekimi Tetikleyici (Gravity)
        GravityButtonTrigger gravityTrigger = GetComponent<GravityButtonTrigger>();
        if (gravityTrigger != null)
        {
            gravityTrigger.ExecuteFlip();
        }

        // Ses Efekti
        if (SoundManager.instance != null)
        {
            SoundManager.PlaySFX(SoundManager.instance.buttonSound);
        }

        // KAPIYI AÇAN KISIM
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
        }
    }

    public void ResetButton()
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
}