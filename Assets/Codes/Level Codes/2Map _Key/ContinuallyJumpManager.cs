using UnityEngine;

/// <summary>
/// Controls the tiring jump mechanic of the player.
/// (Oyuncunun yorulan zıplama mekaniğini kontrol eder.)
/// </summary>
public class TiringJumpRule : MonoBehaviour, IResettable 
{
    public static TiringJumpRule Instance { get; private set; }

    [Header("Jump Settings (Zıplama Ayarları)")]
    public float startingForce = 15f;
    public float fatigueAmount = 1.5f;
    public float minimumForce = 4f;

    [Header("Fall Support (Zıplama Desteği)")]
    public float fallBoostMultiplier = 2.5f;
    public float maxForce = 20f;
    public float minFallDistance = 0.8f;

    [Header("Auto Jump (Otomatik Zıplama)")]
    public float jumpCooldown = 0.1f;
    public float minSoundForce = 5f;

    private float currentForce;
    private float lastGroundY;
    private float lastJumpTime;
    private bool wasGrounded = false;

    // Performans için arama gecikmesi değişkenleri
    private float playerSearchCooldown = 0.5f;
    private float lastSearchTime = 0f;

    private Rigidbody2D playerRb;
    private PlayerController player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentForce = startingForce;
    }

    /// <summary>
    /// Obje sahneye yüklendiğinde kendini LevelManager'ın sıfırlama listesine ekler.
    /// </summary>
    private void Start()
    {
        //Event dinlemek yerine doğrudan yöneticiye kayıt oluyoruz
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        FindPlayer();
    }

    /// <summary>
    /// Obje sahneden silindiğinde hafıza sızıntısını önlemek için listeden çıkar.
    /// </summary>
    private void OnDestroy()
    {
        // Silinirken kaydı siliyoruz ki oyun çökmesin
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    private void Update()
    {
        // Performans optimizasyonu: Oyuncu yoksa saniyede 60 kez değil, yarım saniyede bir ara
        if (playerRb == null || player == null)
        {
            if (Time.time > lastSearchTime + playerSearchCooldown)
            {
                FindPlayer();
                lastSearchTime = Time.time;
            }
            return;
        }

        if (player.isGrounded && !wasGrounded)
        {
            OnLanded();
        }

        if (player.isGrounded && Time.time > lastJumpTime + jumpCooldown)
        {
            AutoJump();
        }

        wasGrounded = player.isGrounded;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            player = playerObj.GetComponent<PlayerController>();
            lastGroundY = playerObj.transform.position.y;
        }
    }

    private void OnLanded()
    {
        float currentY = player.transform.position.y;
        float heightDifference = lastGroundY - currentY;

        if (heightDifference > minFallDistance)
        {
            float gainedForce = heightDifference * fallBoostMultiplier;
            currentForce += gainedForce;
        }
        else
        {
            currentForce -= fatigueAmount;
        }

        currentForce = Mathf.Clamp(currentForce, minimumForce, maxForce);
        lastGroundY = currentY;
    }

    private void AutoJump()
    {
        lastJumpTime = Time.time;
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
        playerRb.AddForce(Vector2.up * currentForce, ForceMode2D.Impulse);

        if (currentForce > minSoundForce && SoundManager.instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Jump, 0.8f);
        }
    }

    /// <summary>
    /// IResettable arayüzünün zorunlu kıldığı sıfırlama fonksiyonu.
    /// (LevelManager tarafından bölüm yeniden başladığında otomatik tetiklenir.)
    /// </summary>
    public void ResetMechanic() //  Fonksiyon ismi arayüze uyduruldu
    {
        currentForce = startingForce;

        if (player != null)
        {
            lastGroundY = player.transform.position.y;
            wasGrounded = true;
        }
    }
}