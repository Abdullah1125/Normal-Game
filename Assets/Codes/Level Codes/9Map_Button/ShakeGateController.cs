using UnityEngine;

/// <summary>
/// Özel seviye mekaniği: Telefon sallandığında ana kapıyı açar.
/// Mobil cihazlar için profesyonelce optimize edilmiştir (Sıfır GC, sqrMagnitude kullanımı).
/// </summary>
public class ShakeGateController : MonoBehaviour, IResettable
{
    [Header("Sallama (Shake) Ayarları")]
    [Tooltip("Telefonu ne kadar şiddetli sallamak gerektiğini belirler. (Standart yerçekimi 1'dir. 2 veya 2.5 idealdir.)")]
    [SerializeField] private float _shakeThreshold = 2.5f; 
    
    // Durum kontrolü
    private bool _isOpen = false;

    // Performans için önbelleklenmiş değişken
    private float _shakeThresholdSqr;
    private GateController _cachedGate;

    private void Awake()
    {
        // Karekök maliyetinden kurtulmak için threshold'un karesini önbelleğe alıyoruz.
        _shakeThresholdSqr = _shakeThreshold * _shakeThreshold;
    }

    private void Start()
    {
        _cachedGate = GateController.Instance;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    private void Update()
    {
        // Kapı zaten açıldıysa okuma yapma
        if (_isOpen) return;

        // İvmeölçer verisi okuması
        bool isShaked = Input.acceleration.sqrMagnitude >= _shakeThresholdSqr;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Sadece editör ve PC'de tuş kontrolü yapılsın (Mobilde gereksiz CPU yükü engellendi)
        if (!isShaked)
        {
            isShaked = Input.GetKeyDown(KeyCode.Space);
        }
#endif

        if (isShaked)
        {
            // GateController null kontrolünü her frame (kare) yapmak yerine sadece sallandığında yapıyoruz.
            // Bu sayede Unity'nin arka plandaki Native C++ objesine her kare erişmesini (overhead) engelliyoruz.
            if (_cachedGate == null) _cachedGate = GateController.Instance;

            if (_cachedGate != null)
            {
                _isOpen = true;
                _cachedGate.OpenGate();
            }
        }
    }

    public void ResetMechanic()
    {
        // Karakter öldüğünde yeniden sallanabilmesi için durumu sıfırla
        // Kapının kendisi zaten IResettable olduğu için GateController kendi kendini sıfırlayacaktır.
        _isOpen = false;
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}
