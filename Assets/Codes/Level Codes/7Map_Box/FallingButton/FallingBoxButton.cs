using UnityEngine;
using System.Collections;

/// <summary>
/// Yukarıdan düşen, düştüğünde kamerayı titreten ve ardından kutu gelince GateController'ı açan buton.
/// Mobil cihazlar için optimize edilmiştir.
/// </summary>
public class FallingBoxButton : MonoBehaviour, IResettable
{
    [Header("Sprites (Görseller)")]
    public Sprite normalSprite;
    public Sprite pressedSprite;
    private SpriteRenderer _sr;

    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem pressParticles;

    [Header("Drop Settings (Düşme Ayarları)")]
    [Tooltip("Butonun düşeceği hedef zemin pozisyonu.")]
    public Transform targetGroundPosition; 
    
    [Tooltip("Butonun yere düşme hızı.")]
    public float dropSpeed = 15f;

    private bool _isDropped = false;
    private bool _isPressed = false;
    private int _objectsOnButton = 0;

    private Vector3 _startPosition; // Level resetlendiğinde butonu tekrar havaya almak için

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (normalSprite != null && _sr != null) _sr.sprite = normalSprite;
        _startPosition = transform.position;
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    /// <summary>
    /// Birinci buton tarafından tetiklenerek düşme animasyonunu başlatır.
    /// </summary>
    public void DropButton()
    {
        if (!_isDropped)
        {
            StartCoroutine(DropAnimation());
        }
    }

    private IEnumerator DropAnimation()
    {
        // Mobil Optimizasyon: Vector3.Distance dahili olarak Mathf.Sqrt (karekök) hesaplar.
        // sqrMagnitude karekök HESAPLAMAZ → mobil CPU'larda çok daha hızlı.
        // 0.01f mesafe kontrolü yerine 0.01f * 0.01f = 0.0001f karşılaştırma yapıyoruz.
        float sqrThreshold = 0.01f * 0.01f;
        while ((transform.position - targetGroundPosition.position).sqrMagnitude > sqrThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetGroundPosition.position, Time.deltaTime * dropSpeed);
            yield return null;
        }

        transform.position = targetGroundPosition.position;
        _isDropped = true;

        // Yere çarptığında oyunun kendi sistemindeki kamerayı titret
        if (CameraRoomController.Instance != null)
        {
            CameraRoomController.Instance.ShakeCamera();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            if (!LevelManager.Instance.activeLevel.isActive) return;
        }

        // Buton yere düşmediyse hiçbir şekilde kutuyu algılama
        if (!_isDropped) return;

        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton++;
            if (!_isPressed) PressButton();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!gameObject.scene.isLoaded || !other.gameObject.activeInHierarchy) return;

        if (other.CompareTag(Constants.TAG_BOX))
        {
            _objectsOnButton--;
            if (_objectsOnButton <= 0 && _isPressed) ReleaseButton();
        }
    }

    private void PressButton()
    {
        _isPressed = true;
        if (_sr != null && pressedSprite != null) _sr.sprite = pressedSprite;

        if (SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Button);
        }

        if (pressParticles != null)
        {
            pressParticles.Stop();
            pressParticles.Play();
        }

        // Kendi GateController'ını tetikle
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
        }
    }

    private void ReleaseButton()
    {
        _isPressed = false;
        if (_sr != null && normalSprite != null) _sr.sprite = normalSprite;

        if (GateController.Instance != null)
        {
            GateController.Instance.CloseGate();
        }
    }

    /// <summary>
    /// Karakter öldüğünde veya bölüm sıfırlandığında IResettable tarafından tetiklenir.
    /// </summary>
    public void ResetMechanic()
    {
        // Eğer animasyon (düşme işlemi) devam ediyorsa onu zorla durdur, yoksa düşmeye devam eder
        StopAllCoroutines();

        // Level restart atıldığında her şeyi baştaki (yukarıdaki) haline getiriyoruz
        _objectsOnButton = 0;
        _isPressed = false;
        _isDropped = false;
        transform.position = _startPosition; 

        if (_sr != null && normalSprite != null) _sr.sprite = normalSprite;
        if (pressParticles != null) pressParticles.Stop();

        // Kapı açıksa kapat
        if (GateController.Instance != null)
        {
            GateController.Instance.CloseGate();
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
