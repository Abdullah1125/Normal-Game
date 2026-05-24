using UnityEngine;
using System.Reflection; // Reflection kullanmak için gerekli

/// <summary>
/// A trap button that forces the gate to start completely open without blink/animation, but closes it smoothly if pressed.
/// (Kapýnýn göz kýrpmadan/animasyonsuz direkt açýk baþlamasýný saðlayan, ancak basýldýðýnda kapýyý kayarak kapatan tuzak butonu.)
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class TrapButton : MonoBehaviour, IResettable
{
    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;

    private SpriteRenderer _sr;
    private bool _isPressed = false;

    // Performans için Reflection alanýný önbelleðe alýyoruz
    private FieldInfo _startPosField;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (normalSprite == null && _sr != null)
        {
            normalSprite = _sr.sprite;
        }

        // Aðýr iþlem olan Reflection tip aramasýný Awake'te bir kez yapýyoruz (Optimized)
        _startPosField = typeof(GateController).GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // Oyun baþlarken kapýyý anýnda açýk konuma ýþýnla (Göz kýrpmaz)
        InstantSnapOpen();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isPressed) return;

        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            TriggerTrap();
        }
    }

    /// <summary>
    /// Activates the trap, changes sprite, and triggers the gate to close smoothly.
    /// (Tuzaðý tetikler, görseli deðiþtirir ve kapýnýn kayarak kapanmasýný saðlar.)
    /// </summary>
    private void TriggerTrap()
    {
        _isPressed = true;

        if (_sr != null && pressedSprite != null)
        {
            _sr.sprite = pressedSprite;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Button);
        }

        if (GateController.Instance != null)
        {
            GateController.Instance.CloseGate();
        }
    }

    /// <summary>
    /// Instantly snaps the gate to its fully open position without waiting a frame, preventing visual glitches.
    /// (Görsel hatalarý önlemek için bir kare beklemeden kapýyý anýnda tamamen açýk konuma ýþýnlar.)
    /// </summary>
    // _startPosField tanýmlamaya ve Awake içinde Reflection yapmaya GEREK KALMADI!

    private void InstantSnapOpen()
    {
        // TEK SATIRDA ÝÞLEM TAMAM!
        GateUtility.SnapGateOpen();
    }

    /// <summary>
    /// Resets the trap button and instantly snaps the gate back to its fully open state upon death, ensuring no blinking.
    /// (Ölüm anýnda butonu sýfýrlar ve kapýyý göz kýrpmadan anýnda açýk konuma ýþýnlar.)
    /// </summary>
    public void ResetMechanic()
    {
        _isPressed = false;

        if (_sr != null)
        {
            _sr.sprite = normalSprite;
            _sr.color = Color.white;
        }

        // HATANIN ÇÖZÜLDÜÐÜ YER: Coroutine bitti. Anýnda ýþýnlama yapýlýyor.
        InstantSnapOpen();

        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
        if (GateController.Instance != null )
        {
            GateController.Instance.ResetMechanic();
           
        }
    }
}