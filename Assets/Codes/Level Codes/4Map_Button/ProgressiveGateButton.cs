using UnityEngine;

/// <summary>
/// A button designed to be jumped on multiple times to progressively open a linked gate.
/// Automatically finds the ProgressiveGateController in the scene.
/// (Baðlý kapýyý aþamalý açmak için üstünde defalarca zýplanmasý gereken buton. Sahnedeki özel kapýyý otomatik bulur.)
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class ProgressiveGateButton : MonoBehaviour, IResettable
{
    [Header("Mechanic Settings (Mekanik Ayarlarý)")]
    public float progressPerJump = 0.2f; // Her zýplamada kapý %20 açýlýr (Tam açýlmasý için 5 kere basmalý)
    public float cooldown = 0.2f; // Animasyon ve spam korumasý için bekleme süresi

    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    private ProgressiveGateController _targetGate;
    private SpriteRenderer _sr;
    private float _lastPressTime;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (normalSprite == null && _sr != null)
        {
            normalSprite = _sr.sprite;
        }
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // --- OTOMATÝK BAÐLANTI: Sahnedeki özel kapýyý kendisi bulur ---
        // (Böylece prefablara Inspector üzerinden sürükle býrak yapmana gerek kalmaz)
        _targetGate = Object.FindFirstObjectByType<ProgressiveGateController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER) && Time.time > _lastPressTime + cooldown)
        {
            PressAction();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            ReleaseAction();
        }
    }

    /// <summary>
    /// Executes the press logic, pushes the gate, and changes the sprite.
    /// (Basýlma mantýðýný çalýþtýrýr, kapýyý iter ve görseli deðiþtirir.)
    /// </summary>
    private void PressAction()
    {
        _lastPressTime = Time.time;

        if (pressedSprite != null) _sr.sprite = pressedSprite;

        // Özel kapýya ilerleme sinyali gönder
        if (_targetGate != null)
        {
            _targetGate.AddProgress(progressPerJump);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Button);
        }
    }

    private void ReleaseAction()
    {
        if (_sr != null && normalSprite != null)
        {
            _sr.sprite = normalSprite;
        }
    }

    public void ResetMechanic()
    {
        ReleaseAction();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}