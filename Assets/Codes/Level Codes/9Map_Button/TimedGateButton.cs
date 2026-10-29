using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Özel seviye mekaniği: Buton kapıyı doğrudan kontrol eder.
/// Mobil cihazlar için GC yaratmayacak şekilde ve sqrMagnitude ile optimize edilmiştir.
/// </summary>
public class TimedGateButton : MonoBehaviour, IResettable
{
    [Header("Gate Settings (Sadece bu level için)")]
    [SerializeField] private Vector3 _moveOffset = new Vector3(0, 5f, 0);
    [SerializeField] private float _openSpeed = 2f;
    [SerializeField] private float _closeSpeed = 0.5f;
    
    [Tooltip("Kapı tam kapanmadan, karakterin geçemeyeceği kadar kapandığında butonun erken açılması için mesafe eşiği.")]
    [SerializeField] private float _reactivateThreshold = 1.2f;

    [Header("Sprites (Görseller)")]
    [SerializeField] private Sprite _normalSprite;
    [SerializeField] private Sprite _pressedSprite;

    [Header("Effects (Efektler)")]
    [SerializeField] private ParticleSystem _pressParticles;
    public UnityEvent OnButtonPressed;

    private SpriteRenderer _sr;
    private PolygonCollider2D _polyCollider;
    
    // Kapı referansı ve pozisyonları
    private Transform _targetGate;
    private Vector3 _startPos;
    private Vector3 _targetPos;
    
    // Durum kontrolü
    private bool _isOpening = false;
    private bool _isActive = true; 
    private int _objectsOnButton = 0;

    // Performans için önbelleğe alınmış eşik değerleri (Karekök hesabından kaçınmak için)
    private float _reactivateThresholdSqr;
    private readonly float _movementThresholdSqr = 0.000001f;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _polyCollider = GetComponent<PolygonCollider2D>();
        
        if (_normalSprite == null && _sr != null)
        {
            _normalSprite = _sr.sprite;
        }
        
        if (_sr != null && _normalSprite != null)
        {
            _sr.sprite = _normalSprite;
        }

        // Karekök maliyetinden kurtulmak için threshold'un karesini alıp önbellekliyoruz
        _reactivateThresholdSqr = _reactivateThreshold * _reactivateThreshold;
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        if (GateController.Instance != null)
        {
            _targetGate = GateController.Instance.transform;
            GateController.Instance.enabled = false; 

            _startPos = _targetGate.position;
            _targetPos = _startPos + _moveOffset;
        }
    }

    private void Update()
    {
        if (_targetGate == null) return;

        Vector3 currentGateTarget = _isOpening ? _targetPos : _startPos;
        
        // C++ Tarafına Geçişleri (Native Binding) Azaltmak:
        // Her frame "transform.position" çağırmak yerine tek bir değişkende saklıyoruz.
        Vector3 gatePos = _targetGate.position;

        float distSqr = (gatePos - currentGateTarget).sqrMagnitude;

        // Performans: Kapı hedefe çoktan ulaştıysa (dinlenme hali) hesaplamaları atla
        if (distSqr <= _movementThresholdSqr)
        {
            // Pozisyonu sadece ufak bir kayma varsa eşitle. 
            // Gereksiz yere her kare değer atamak Unity'de physics/render dirty bayrağını tetikler (aşırı yük)
            if (gatePos != currentGateTarget)
            {
                _targetGate.position = currentGateTarget;
            }

            if (!_isOpening && !_isActive)
            {
                ActivateButtonVisuals();
            }
            return; // Kalan işlemleri atla
        }

        // Eğer buraya indiysek kapı hareket ediyordur.
        float currentSpeed = _isOpening ? _openSpeed : _closeSpeed;
        gatePos = Vector3.MoveTowards(gatePos, currentGateTarget, currentSpeed * Time.deltaTime);
        
        // Sadece pozisyon gerçekten değiştiğinde atama yap (1 kez native çağrı)
        _targetGate.position = gatePos;

        if (!_isOpening && !_isActive)
        {
            if ((gatePos - _startPos).sqrMagnitude <= _reactivateThresholdSqr)
            {
                ActivateButtonVisuals();
            }
        }
    }

    private void ActivateButtonVisuals()
    {
        _isActive = true;
        
        if (_objectsOnButton > 0)
        {
            PressButton();
            return;
        }

        if (_sr != null && _normalSprite != null) 
        {
            _sr.sprite = _normalSprite;
            _sr.color = Color.white;
        }
        UpdateCollider();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            _objectsOnButton++;
            
            if (_isActive && _objectsOnButton == 1)
            {
                PressButton();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            _objectsOnButton--;
            if (_objectsOnButton < 0) _objectsOnButton = 0;

            if (_objectsOnButton == 0 && _isOpening)
            {
                ReleaseButton();
            }
        }
    }

    private void PressButton()
    {
        _isOpening = true;

        if (_pressParticles != null) _pressParticles.Play();
        if (SoundManager.Instance != null) SoundManager.PlayThemeSFX(SFXType.Button);

        if (_sr != null && _pressedSprite != null)
        {
            _sr.sprite = _pressedSprite;
            UpdateCollider();
        }

        if (SoundManager.Instance != null && gameObject.activeInHierarchy)
        {
            SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
        }

        OnButtonPressed?.Invoke();
    }

    private void ReleaseButton()
    {
        _isOpening = false; 
        _isActive = false;  

        if (_sr != null && _pressedSprite != null)
        {
            _sr.sprite = _pressedSprite; 
            _sr.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
        }
        
        if (SoundManager.Instance != null && gameObject.activeInHierarchy)
        {
            SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
        }
    }

    public void ResetMechanic()
    {
        _objectsOnButton = 0;
        _isOpening = false;
        
        if (_targetGate != null)
        {
            _targetGate.position = _startPos;
        }

        ActivateButtonVisuals();
    }

    private void UpdateCollider()
    {
        if (_polyCollider != null)
        {
            _polyCollider.pathCount = 0; 
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }

        if (GateController.Instance != null)
        {
            GateController.Instance.enabled = true;
        }
    }
}
