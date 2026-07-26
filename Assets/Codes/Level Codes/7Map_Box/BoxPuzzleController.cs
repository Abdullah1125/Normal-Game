using UnityEngine;

/// <summary>
/// Karakter belirtilen X çizgisinden geçtiğinde kutunun kaymasını sağlayan mekanizma.
/// İlk kayma yönünü kutunun butondan en kısa yoldan çıkabileceği yöne göre (merkeze olan yakınlık) belirler.
/// </summary>
[RequireComponent(typeof(Box))]
[RequireComponent(typeof(Rigidbody2D))]
public class BoxPuzzleController : MonoBehaviour, IResettable
{
    [Header("Mekanik Ayarları")]
    [Tooltip("Karakterin (Oyuncunun) hangi X noktasını geçince kutunun harekete geçeceği.")]
    public float targetX = 0f;

    [Tooltip("Kutunun butondan çıkarkenki yatay fırlatılma hızı.")]
    public float autoSlideSpeed = 5f;

    [Tooltip("Kutunun butondan çıkarkenki dikey (yukarı doğru) fırlatılma gücü.")]
    public float throwUpForce = 7f;

    // Bileşenler
    private Box boxComponent;
    private Rigidbody2D rb;
    private Transform playerTransform;

    // Durum Değişkenleri
    private int passCount = 0;
    private bool isAutoSliding = false;
    private int slideDirection = 1;
    private int firstSlideDirection = 1; // İlk kaydığı yönü hatırlar ki 2. geçişte tam tersine gitsin
    private bool wasPlayerRightOfTarget = false;
    private float targetExitX = 0f;

    // Buton Takip Değişkenleri
    private bool isOnButton = false;
    private Transform currentButton; // Üzerinde durduğumuz butonun referansı

    void Awake()
    {
        boxComponent = GetComponent<Box>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag(Constants.TAG_PLAYER);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            wasPlayerRightOfTarget = playerTransform.position.x > targetX;
        }
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        // 1. OTOMATİK KAYMA DURUMU (Artık fırlatılma durumu)
        if (isAutoSliding)
        {
            // Kutunun hedef (güvenli) X konumuna ulaşıp ulaşmadığını kontrol et
            bool hasClearedTarget = (slideDirection > 0) 
                ? (transform.position.x >= targetExitX) 
                : (transform.position.x <= targetExitX);

            if (hasClearedTarget)
            {
                isAutoSliding = false;
                wasPlayerRightOfTarget = playerTransform.position.x > targetX;
                boxComponent.enabled = true;
                
                // Tam sınırda yatay hızı sıfırla ki havada daha fazla ileri savrulmasın
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                // Butonun etkileşiminden kurtulana kadar yatay hızı zorla
                rb.linearVelocity = new Vector2(slideDirection * autoSlideSpeed, rb.linearVelocity.y);
            }
            return;
        }

        // 3 Kere geçtiyse kilitlenmiştir
        if (passCount >= 3) return;

        // 2. OYUNCUNUN ÇİZGİYİ GEÇME KONTROLÜ
        bool isPlayerRightOfTarget = playerTransform.position.x > targetX;

        if (isPlayerRightOfTarget != wasPlayerRightOfTarget)
        {
            wasPlayerRightOfTarget = isPlayerRightOfTarget;

            // SADECE KUTU BİR BUTONUN ÜZERİNDEYSE İŞLEM YAP
            if (isOnButton)
            {
                passCount++;

                if (passCount == 1)
                {
                    // İLK GEÇİŞ: Butonun neresinde durduğuna bak (Sağa mı daha yakın Sola mı?)
                    int direction = 1;
                    if (currentButton != null)
                    {
                        // Kutunun X'i butonun X'inden büyükse sağa daha yakındır, değilse sola yakındır.
                        direction = (transform.position.x >= currentButton.position.x) ? 1 : -1;
                    }

                    firstSlideDirection = direction; // Yönü hafızaya al
                    StartAutoSlide(firstSlideDirection);
                }
                else if (passCount == 2)
                {
                    // İKİNCİ GEÇİŞ: İlk kaydığı yönün tam tersine kaydır
                    StartAutoSlide(-firstSlideDirection);
                }
                else if (passCount == 3)
                {
                    // ÜÇÜNCÜ GEÇİŞ: Kutuyu Tamamen Kilitle
                    boxComponent.enabled = false;
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.constraints = RigidbodyConstraints2D.FreezeAll;
                }
            }
        }
    }

    private void StartAutoSlide(int direction)
    {
        isAutoSliding = true;
        slideDirection = direction;

        boxComponent.enabled = false;
        
        // Hedef X konumunu hesapla ki zıpladığında aktifleşmeyeceği (garanti dışarı çıkacağı) noktayı bilelim
        if (currentButton != null)
        {
            Collider2D btnCollider = currentButton.GetComponent<Collider2D>();
            Collider2D myCollider = GetComponent<Collider2D>();
            
            float btnWidth = btnCollider != null ? btnCollider.bounds.extents.x : 1f;
            float myWidth = myCollider != null ? myCollider.bounds.extents.x : 0.5f;
            
            // Güvenli mesafe: butonun yarısı + kutunun yarısı + milimetrik bir boşluk (0.1f)
            float requiredDistance = btnWidth + myWidth + 0.1f;
            targetExitX = currentButton.position.x + (direction * requiredDistance);
        }
        else
        {
            targetExitX = transform.position.x + (direction * 1f);
        }
        
        // Kutuyu belirlenen yatay yönde ve yukarı doğru fırlat
        rb.linearVelocity = new Vector2(direction * autoSlideSpeed, throwUpForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<BoxButton>() != null)
        {
            isOnButton = true;
            currentButton = other.transform; // Butonun objesini (ve merkez pozisyonunu) hafızaya al
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<BoxButton>() != null)
        {
            isOnButton = false;
            currentButton = null; // Butondan çıkınca referansı temizle
        }
    }

    public void ResetMechanic()
    {
        passCount = 0;
        isAutoSliding = false;
        isOnButton = false;
        currentButton = null;

        boxComponent.enabled = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (playerTransform != null)
        {
            wasPlayerRightOfTarget = playerTransform.position.x > targetX;
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
