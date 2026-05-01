using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// Obje sahnede doðduðunda oyuncuyu raylý sisteme geçirir.
/// Rota mesafeleri Start'ta ön belleðe alýnarak (Caching) maksimum CPU optimizasyonu saðlanmýþtýr.
/// </summary>
public class OnRailsRhythmicCurse : MonoBehaviour
{
    [Header("Route Settings (Rota Ayarlarý)")]
    public Transform[] waypoints;
    public float stepDistance = 2f;
    public float travelSpeed = 15f;
    public bool releaseAtEnd = true;

    private float _targetDistance = 0f;
    private float _currentDistance = 0f;
    private float _totalPathLength = 0f;
    private float[] _segmentLengths; // SÝHÝR BURADA: Mesafeleri hafýzada tutacaðýmýz dizi
    private bool _waitingForRight = false;

    private Rigidbody2D _playerRb;
    private float _origMoveSpeed;
    private float _origDefaultSpeed;
    private float _origJump1;
    private float _origJump2;
    private float _origGravity;

    private List<MobileButtonListener> _injectedListeners = new List<MobileButtonListener>();

    void Start()
    {
        if (PlayerController.Instance == null) return;

        _playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();

        _origMoveSpeed = PlayerController.Instance.moveSpeed;
        _origDefaultSpeed = PlayerController.Instance.defaultSpeed;
        _origJump1 = PlayerController.Instance.firstJumpForce;
        _origJump2 = PlayerController.Instance.doubleJumpForce;
        _origGravity = _playerRb.gravityScale;

        PlayerController.Instance.moveSpeed = 0f;
        PlayerController.Instance.defaultSpeed = 0f;
        PlayerController.Instance.firstJumpForce = 0f;
        PlayerController.Instance.doubleJumpForce = 0f;

        _playerRb.gravityScale = 0f;
        _playerRb.linearVelocity = Vector2.zero;

        // Rota mesafelerini BÝR KERE hesapla ve hafýzaya al
        CalculateAndCachePathLengths();

        if (waypoints != null && waypoints.Length > 0)
        {
            PlayerController.Instance.transform.position = waypoints[0].position;
        }

        // Mobil butonlara ajan enjekte et
        MobileDirectionButton[] mobileButtons = FindObjectsByType<MobileDirectionButton>(FindObjectsSortMode.None);
        foreach (MobileDirectionButton btn in mobileButtons)
        {
            MobileButtonListener listener = btn.gameObject.AddComponent<MobileButtonListener>();
            listener.Setup(btn.isLeftButton, this);
            _injectedListeners.Add(listener);
        }
    }

    void Update()
    {
        if (_playerRb == null || !PlayerController.Instance.canMove) return;

        _playerRb.linearVelocity = Vector2.zero;

        // PC Klavye Kontrolü
        if (Input.GetKeyDown(KeyCode.A)) TryStep(-1);
        else if (Input.GetKeyDown(KeyCode.D)) TryStep(1);

        _targetDistance = Mathf.Clamp(_targetDistance, 0, _totalPathLength);
        _currentDistance = Mathf.MoveTowards(_currentDistance, _targetDistance, travelSpeed * Time.deltaTime);

        // Karakterin pozisyonunu güncelle
        UpdatePlayerPositionAlongPath(_currentDistance);

        if (releaseAtEnd && _currentDistance >= _totalPathLength)
        {
            Destroy(gameObject);
        }
    }

    public void TryStep(int direction)
    {
        if (!_waitingForRight && direction == -1)
        {
            _targetDistance += stepDistance;
            _waitingForRight = true;
        }
        else if (_waitingForRight && direction == 1)
        {
            _targetDistance += stepDistance;
            _waitingForRight = false;
        }
    }

    /// <summary>
    /// Aðýr matematik hesaplamalarýný oyun baþýnda sadece bir kez yapar.
    /// </summary>
    private void CalculateAndCachePathLengths()
    {
        _totalPathLength = 0f;

        if (waypoints == null || waypoints.Length < 2) return;

        // Dizi boyutunu ayarla (Nokta sayýsýndan 1 eksik kadar aralýk vardýr)
        _segmentLengths = new float[waypoints.Length - 1];

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            float dist = Vector2.Distance(waypoints[i].position, waypoints[i + 1].position);
            _segmentLengths[i] = dist; // Hafýzaya kaydet
            _totalPathLength += dist;  // Toplama ekle
        }
    }

    /// <summary>
    /// Update içinde çalýþýr ama aðýr hesaplama yapmaz, hafýzadaki (Cache) veriyi okur.
    /// </summary>
    private void UpdatePlayerPositionAlongPath(float distance)
    {
        if (waypoints == null || waypoints.Length < 2) return;

        float accumulatedDistance = 0f;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            // SÝHÝR BURADA: Aðýr Vector2.Distance yerine hafýzadaki hazýr sayýyý çektik
            float segmentLength = _segmentLengths[i];

            if (distance <= accumulatedDistance + segmentLength)
            {
                // Sadece gereken noktada kýsa bir bölme iþlemi (Lerp yüzdesi için)
                float t = (distance - accumulatedDistance) / segmentLength;
                PlayerController.Instance.transform.position = Vector2.Lerp(waypoints[i].position, waypoints[i + 1].position, t);
                return;
            }
            accumulatedDistance += segmentLength;
        }

        PlayerController.Instance.transform.position = waypoints[waypoints.Length - 1].position;
    }

    void OnDestroy()
    {
        if (PlayerController.Instance != null && _playerRb != null)
        {
            PlayerController.Instance.moveSpeed = _origMoveSpeed;
            PlayerController.Instance.defaultSpeed = _origDefaultSpeed;
            PlayerController.Instance.firstJumpForce = _origJump1;
            PlayerController.Instance.doubleJumpForce = _origJump2;
            _playerRb.gravityScale = _origGravity;
        }

        foreach (MobileButtonListener listener in _injectedListeners)
        {
            if (listener != null) Destroy(listener);
        }
    }
}

// =========================================================================
// MOBÝL BUTONLARA SIZAN AJAN SCRÝPTÝ (Agent Listener)
// =========================================================================
public class MobileButtonListener : MonoBehaviour, IPointerDownHandler
{
    private bool _isLeft;
    private OnRailsRhythmicCurse _curseManager;

    public void Setup(bool isLeft, OnRailsRhythmicCurse curseManager)
    {
        _isLeft = isLeft;
        _curseManager = curseManager;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_curseManager != null)
        {
            _curseManager.TryStep(_isLeft ? -1 : 1);
        }
    }
}