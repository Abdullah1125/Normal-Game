using UnityEngine;

/// <summary>
/// Karakter belirlenen X koordinatını geçtiğinde kutunun otomatik kaymasını sağlayan bulmaca mekanizması.
/// Kutunun butondan en kısa yoldan (merkeze olan mesafeye göre) çıkabileceği yönü hesaplar.
/// Mobil cihazlar için GetComponent çağrıları önbelleğe (cache) alınarak optimize edilmiştir.
/// </summary>
[RequireComponent(typeof(Box))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))] // Kutu boyutunu ölçmek için eklendi
public class BoxPuzzleController : MonoBehaviour, IResettable
{
    [Header("Mekanik Ayarları")]
    [Tooltip("Karakterin bu X noktasını geçmesi kutuyu harekete geçirir.")]
    public float targetX = 0f;

    [Tooltip("Kutunun butondan çıkarken fırlatılacağı yatay hız.")]
    public float autoSlideSpeed = 5f;

    [Tooltip("Kutunun fırlatılırken havaya doğru uygulanacak dikey gücü.")]
    public float throwUpForce = 7f;

    // Önbelleğe Alınan Bileşenler (Cache)
    private Box _boxComponent;
    private Rigidbody2D _rb;
    private Collider2D _myCollider;
    private Transform _playerTransform;

    // Mekanik Durum Değişkenleri
    private int _passCount = 0;
    private bool _isAutoSliding = false;
    private int _slideDirection = 1;
    private int _firstSlideDirection = 1; // 2. geçişte zıt yöne gitmesi için ilk yönü hatırlar
    private bool _wasPlayerRightOfTarget = false;
    private float _targetExitX = 0f;

    // Buton Etkileşim Değişkenleri
    private bool _isOnButton = false;
    private Transform _currentButton;
    private Collider2D _currentButtonCollider; // Her fırlatmada GetComponent yapmamak için önbelleklendi

    private void Awake()
    {
        // Bileşenleri sadece bir kere çağırarak belleğe al (Mobil Optimizasyon)
        _boxComponent = GetComponent<Box>();
        _rb = GetComponent<Rigidbody2D>();
        _myCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // Oyuncuyu bul ve başlangıç durumunu kaydet
        GameObject playerObj = GameObject.FindGameObjectWithTag(Constants.TAG_PLAYER);
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
            _wasPlayerRightOfTarget = _playerTransform.position.x > targetX;
        }
    }

    private void FixedUpdate()
    {
        if (_playerTransform == null) return;

        // 1. OTOMATİK KAYMA DURUMU KONTROLÜ
        if (_isAutoSliding)
        {
            // Kutunun hedef x eksenine ulaşıp ulaşmadığını kontrol et
            bool hasClearedTarget = (_slideDirection > 0) 
                ? (transform.position.x >= _targetExitX) 
                : (transform.position.x <= _targetExitX);

            if (hasClearedTarget)
            {
                // Hedefe ulaşıldı, mekaniği normal haline döndür
                _isAutoSliding = false;
                _wasPlayerRightOfTarget = _playerTransform.position.x > targetX;
                _boxComponent.enabled = true;
                
                // Kutu havada daha fazla sürüklenmesin diye yatay hızı sıfırla
                _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            }
            else
            {
                // Hedefe ulaşana kadar yatay hızı sabit tut
                _rb.linearVelocity = new Vector2(_slideDirection * autoSlideSpeed, _rb.linearVelocity.y);
            }
            return; // Kayma bitene kadar aşağıdaki kodları okumasına gerek yok
        }

        // 3 kere çalıştıysa artık mekanik kilitlenir
        if (_passCount >= 3) return;

        // 2. OYUNCUNUN HEDEF NOKTAYI GEÇME KONTROLÜ
        bool isPlayerRightOfTarget = _playerTransform.position.x > targetX;

        if (isPlayerRightOfTarget != _wasPlayerRightOfTarget)
        {
            _wasPlayerRightOfTarget = isPlayerRightOfTarget;

            // Kutunun herhangi bir buton üzerinde olması şartı
            if (_isOnButton)
            {
                _passCount++;

                if (_passCount == 1)
                {
                    // İLK GEÇİŞ: Kutunun konumu butonun sağına mı soluna mı daha yakın?
                    int direction = 1;
                    if (_currentButton != null)
                    {
                        direction = (transform.position.x >= _currentButton.position.x) ? 1 : -1;
                    }

                    _firstSlideDirection = direction; 
                    StartAutoSlide(_firstSlideDirection);
                }
                else if (_passCount == 2)
                {
                    // İKİNCİ GEÇİŞ: İlk yöne göre tam tersi yöne fırlat
                    StartAutoSlide(-_firstSlideDirection);
                }
                else if (_passCount == 3)
                {
                    // ÜÇÜNCÜ GEÇİŞ: Sistemi tamamen dondur ve kilitle
                    _boxComponent.enabled = false;
                    _rb.linearVelocity = Vector2.zero;
                    _rb.bodyType = RigidbodyType2D.Kinematic;
                    _rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }
            }
        }
    }

    /// <summary>
    /// Kutunun belirtilen yöne doğru havaya fırlatılma işlemini başlatır.
    /// </summary>
    private void StartAutoSlide(int direction)
    {
        _isAutoSliding = true;
        _slideDirection = direction;

        // Havada oyuncu tarafından müdahale edilmesini engelle
        _boxComponent.enabled = false;
        
        // Zıpladığında aktifleşmemesi (kesin çıkış) için gereken güvenli mesafeyi hesapla
        if (_currentButton != null && _currentButtonCollider != null && _myCollider != null)
        {
            float btnWidth = _currentButtonCollider.bounds.extents.x;
            float myWidth = _myCollider.bounds.extents.x;
            
            // Güvenli mesafe: Butonun yarısı + Kutunun yarısı + ufak tolerans payı
            float requiredDistance = btnWidth + myWidth + 0.1f;
            _targetExitX = _currentButton.position.x + (direction * requiredDistance);
        }
        else
        {
            // Fallback (Bileşen eksikse varsayılan değer)
            _targetExitX = transform.position.x + (direction * 1f);
        }
        
        // Kutuyu fırlat
        _rb.linearVelocity = new Vector2(direction * autoSlideSpeed, throwUpForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Mobil Optimizasyon: GetComponent yerine bellek dostu TryGetComponent kullanımı
        if (other.TryGetComponent(out BoxButton btn))
        {
            _isOnButton = true;
            _currentButton = other.transform;
            _currentButtonCollider = other; // Daha sonra tekrar GetComponent yapmamak için sakla
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out BoxButton btn))
        {
            _isOnButton = false;
            _currentButton = null;
            _currentButtonCollider = null;
        }
    }

    public void ResetMechanic()
    {
        _passCount = 0;
        _isAutoSliding = false;
        _isOnButton = false;
        _currentButton = null;
        _currentButtonCollider = null;

        _boxComponent.enabled = true;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (_playerTransform != null)
        {
            _wasPlayerRightOfTarget = _playerTransform.position.x > targetX;
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}
