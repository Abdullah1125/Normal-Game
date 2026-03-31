using UnityEngine;

public class GateButton : MonoBehaviour
{
    [Header("Settings")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.green;
    public Color disabledColor = new Color(0.3f, 0.3f, 0.3f);
    public float pressedScaleY = 0.6f;

    private Vector3 originalScale;
    private bool isPressed = false;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        if (sr != null) sr.color = normalColor;
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
        // Enter kaçýrýlýrsa Stay yakalar
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
        // Level aktif mi kontrol et
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

        // Gravity trigger varsa çalýþtýr
        GravityButtonTrigger gravityTrigger = GetComponent<GravityButtonTrigger>();
        if (gravityTrigger != null)
        {
            gravityTrigger.ExecuteFlip();
        }
        SoundManager.PlaySFX(SoundManager.instance.buttonSound);
        // Kapýyý aç
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
        }

        // Görsel efekt
        if (sr != null) sr.color = pressedColor;
        transform.localScale = new Vector3(originalScale.x, originalScale.y * pressedScaleY, originalScale.z);
    }

    public void ResetButton()
    {
        isPressed = false;
        if (sr != null) sr.color = normalColor;
        transform.localScale = originalScale;
        gameObject.SetActive(true);
    }
}