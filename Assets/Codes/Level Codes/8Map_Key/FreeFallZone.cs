using UnityEngine;

/// <summary>
/// Obje sahnede doğduğunda karaktere "Serbest Düşüş" laneti uygular.
/// Yer çekimi ölçeği düşürülür, hızlı düşüş yavaşlatılır ve düşüş hızı sınırlandırılır.
/// Çift zıplama hakkı iptal edilir. Obje yok edildiğinde tüm değerler eski haline döner.
/// </summary>
public class FreeFallCurse : MonoBehaviour, IResettable
{
    [Header("Serbest Düşüş Ayarları")]
    [Range(0.1f, 3f)]
    public float curseGravityScale = 0.7f;

    public float maxFallSpeed = -3f;

    [Range(1f, 5f)]
    public float curseFallMultiplier = 1f;

    private float _origGravityScale;
    private float _origFallMultiplier;
    private int _origExtraJumps;
    private Rigidbody2D _playerRb;
    private bool _isActive = false;

    /// <summary>
    /// Oyuncunun orijinal değerlerini kaydeder ve laneti uygular.
    /// </summary>
    void Start()
    {
        if (PlayerController.Instance == null) return;

        _playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
        if (_playerRb == null) return;

        _origGravityScale = _playerRb.gravityScale;
        _origFallMultiplier = PlayerController.Instance.fallMultiplier;
        _origExtraJumps = PlayerController.Instance.extraJumpsValue;

        Apply();
    }

    /// <summary>
    /// Lanet değerlerini oyuncuya uygular.
    /// </summary>
    private void Apply()
    {
        if (_playerRb != null)
            _playerRb.gravityScale = curseGravityScale;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.fallMultiplier = curseFallMultiplier;
            PlayerController.Instance.extraJumpsValue = 0;
        }

        _isActive = true;
    }

    /// <summary>
    /// Her fizik adımında düşüş hızını maxFallSpeed ile sınırlandırır.
    /// Yer çekimi yönüne göre hem normal hem ters yerçekiminde çalışır.
    /// </summary>
    void FixedUpdate()
    {
        if (!_isActive || _playerRb == null) return;

        float gravityDir = Mathf.Sign(Physics2D.gravity.y);

        bool isFalling = (gravityDir < 0 && _playerRb.linearVelocity.y < 0) ||
                         (gravityDir > 0 && _playerRb.linearVelocity.y > 0);

        if (isFalling)
        {
            if (gravityDir < 0 && _playerRb.linearVelocity.y < maxFallSpeed)
                _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, maxFallSpeed);
            else if (gravityDir > 0 && _playerRb.linearVelocity.y > -maxFallSpeed)
                _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, -maxFallSpeed);
        }
    }

    /// <summary>
    /// IResettable: Oyuncu öldüğünde değerleri sıfırlar ve laneti yeniden uygular.
    /// </summary>
    public void ResetMechanic()
    {
        Restore();
        Apply();
    }

    /// <summary>
    /// Obje sahneden kaldırıldığında oyuncunun orijinal değerlerini geri yükler.
    /// </summary>
    private void OnDestroy()
    {
        Restore();
    }

    /// <summary>
    /// Oyuncunun yer çekimi ölçeğini, düşüş çarpanını ve çift zıplama hakkını eski haline döndürür.
    /// </summary>
    private void Restore()
    {
        if (!_isActive) return;

        if (_playerRb != null)
            _playerRb.gravityScale = _origGravityScale;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.fallMultiplier = _origFallMultiplier;
            PlayerController.Instance.extraJumpsValue = _origExtraJumps;
        }

        _isActive = false;
    }
}
