using UnityEngine;
using System.Reflection;

/// <summary>
/// Swaps the player's color on a timer and externally inverts inputs using Reflection.
/// Does NOT require any modifications to core scripts.
/// (Zamanlayýcýya baðlý olarak rengi deðiþtirir ve girdileri dýþarýdan tersine çevirir. Ana kodlarda deðiþiklik gerektirmez.)
/// </summary>
public class ColorChaosController : MonoBehaviour, IResettable
{
    [Header("Chaos Settings (Kaos Ayarlarý)")]
    public float swapInterval = 3f;
    public Color normalColor = Color.white;
    public Color chaosColor = Color.red;

    private float _timer;
    private bool _isInverted = false;
    private SpriteRenderer _playerSR;

    // Mobil butonlarýn gizli durumlarýný okumak için Reflection alanlarý
    private FieldInfo _leftMobileField;
    private FieldInfo _rightMobileField;

    /// <summary>
    /// Gets references and prepares Reflection to read private static mobile button states.
    /// (Referanslarý alýr ve gizli mobil buton durumlarýný okumak için Reflection hazýrlar.)
    /// </summary>
    void Awake()
    {
        // MobileDirectionButton içindeki 'private static' þalterlere sýzýyoruz
        _leftMobileField = typeof(MobileDirectionButton).GetField("leftPressed", BindingFlags.NonPublic | BindingFlags.Static);
        _rightMobileField = typeof(MobileDirectionButton).GetField("rightPressed", BindingFlags.NonPublic | BindingFlags.Static);
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        if (PlayerController.Instance != null)
        {
            _playerSR = PlayerController.Instance.GetComponent<SpriteRenderer>();
        }

        ResetMechanic();
    }

    /// <summary>
    /// Runs the chaos timer.
    /// (Kaos sayacýný çalýþtýrýr.)
    /// </summary>
    void Update()
    {
        if (PlayerController.Instance == null || !PlayerController.Instance.canMove) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            ToggleChaos();
            _timer = swapInterval;
        }
    }

    /// <summary>
    /// Swaps the chaos state and updates the player's color.
    /// (Kaos durumunu deðiþtirir ve oyuncunun rengini günceller.)
    /// </summary>
    private void ToggleChaos()
    {
        _isInverted = !_isInverted;

        if (_playerSR != null)
        {
            _playerSR.color = _isInverted ? chaosColor : normalColor;
        }

        if (SoundManager.Instance != null)
        {
            // Ýstersen renk deðiþtiði an buraya ses ekleyebilirsin
            // SoundManager.PlayThemeSFX(SFXType.Button); 
        }
    }

    /// <summary>
    /// Intercepts both PC and Mobile inputs externally and applies inverted movement.
    /// (Hem PC hem Mobil girdilerini dýþarýdan yakalar ve tersine hareketi uygular.)
    /// </summary>
    void LateUpdate()
    {
        if (!_isInverted || PlayerController.Instance == null) return;

        float intendedDir = 0f;

        // 1. Önce PC (Klavye) girdisine bak
        float kbInput = Input.GetAxisRaw("Horizontal");
        if (kbInput != 0)
        {
            intendedDir = kbInput;
        }
        // 2. PC girdisi yoksa, sýzdýðýmýz Mobil butonlara bak
        else if (_leftMobileField != null && _rightMobileField != null)
        {
            bool isLeft = (bool)_leftMobileField.GetValue(null);
            bool isRight = (bool)_rightMobileField.GetValue(null);

            if (isLeft && !isRight) intendedDir = -1f;
            else if (isRight && !isLeft) intendedDir = 1f;
        }

        // Eðer oyuncu bir yöne gitmeye çalýþýyorsa, onu tam tersi yöne zorla!
        if (intendedDir != 0)
        {
            float invertedDir = intendedDir * -1f;

            // Ters yönü senin orijinal Move() fonksiyonuna yediriyoruz
            PlayerController.Instance.Move(invertedDir);

            // Karakterin yüzünü de ters yöne bakacak þekilde zorla çeviriyoruz
            if (_playerSR != null)
            {
                float gravityDir = Mathf.Sign(Physics2D.gravity.y);
                _playerSR.flipX = invertedDir < 0 ? !(gravityDir > 0) : (gravityDir > 0);
            }
        }
    }

    /// <summary>
    /// Resets the mechanic upon death.
    /// (Ölüm anýnda mekaniði sýfýrlar.)
    /// </summary>
    public void ResetMechanic()
    {
        _timer = swapInterval;
        _isInverted = false;

        if (_playerSR != null)
        {
            _playerSR.color = normalColor;
        }

        // Yeniden doðduðunda mobil butonlarýn anlýk durumunu güncelle
        MobileDirectionButton.UpdateMovement();

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