using UnityEngine;

/// <summary>
/// Controls the tiring jump mechanic of the player.
/// (Oyuncunun yorulan zÃ„Â±plama mekaniÃ„Å¸ini kontrol eder.)
/// </summary>
public class TiringJumpRule : Singleton<TiringJumpRule>, IResettable 
{

    [Header("Jump Settings (ZÃ„Â±plama AyarlarÃ„Â±)")]
    public float startingForce = 15f;
    public float fatigueAmount = 1.5f;
    public float minimumForce = 4f;

    [Header("Fall Support (ZÃ„Â±plama DesteÃ„Å¸i)")]
    public float fallBoostMultiplier = 2.5f;
    public float maxForce = 20f;
    public float minFallDistance = 0.8f;

    [Header("Auto Jump (Otomatik ZÃ„Â±plama)")]
    public float jumpCooldown = 0.1f;
    public float minSoundForce = 5f;

    private float currentForce;
    private float lastGroundY;
    private float lastJumpTime;
    private bool wasGrounded = false;

    private Rigidbody2D playerRb;
    private PlayerController player;

    protected override void Awake()
    {
        base.Awake();
        currentForce = startingForce;
    }

    /// <summary>
    /// Obje sahneye yÃƒÂ¼klendiÃ„Å¸inde kendini LevelManager'Ã„Â±n sÃ„Â±fÃ„Â±rlama listesine ekler.
    /// </summary>
    private void Start()
    {
        //Event dinlemek yerine doÃ„Å¸rudan yÃƒÂ¶neticiye kayÃ„Â±t oluyoruz
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // Singleton üzerinden oyuncu referansını al
        if (PlayerController.Instance != null)
        {
            player = PlayerController.Instance;
            playerRb = player.GetComponent<Rigidbody2D>();
            lastGroundY = player.transform.position.y;
        }
    }

    /// <summary>
    /// Obje sahneden silindiÃ„Å¸inde hafÃ„Â±za sÃ„Â±zÃ„Â±ntÃ„Â±sÃ„Â±nÃ„Â± ÃƒÂ¶nlemek iÃƒÂ§in listeden ÃƒÂ§Ã„Â±kar.
    /// </summary>
    private void OnDestroy()
    {
        // Silinirken kaydÃ„Â± siliyoruz ki oyun ÃƒÂ§ÃƒÂ¶kmesin
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }

    private void Update()
    {
        if (playerRb == null || player == null) return;

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

        if (currentForce > minSoundForce && SoundManager.Instance != null)
        {
            SoundManager.PlayThemeSFX(SFXType.Jump, 0.8f);
        }
    }

    /// <summary>
    /// IResettable arayÃƒÂ¼zÃƒÂ¼nÃƒÂ¼n zorunlu kÃ„Â±ldÃ„Â±Ã„Å¸Ã„Â± sÃ„Â±fÃ„Â±rlama fonksiyonu.
    /// (LevelManager tarafÃ„Â±ndan bÃƒÂ¶lÃƒÂ¼m yeniden baÃ…Å¸ladÃ„Â±Ã„Å¸Ã„Â±nda otomatik tetiklenir.)
    /// </summary>
    public void ResetMechanic() //  Fonksiyon ismi arayÃƒÂ¼ze uyduruldu
    {
        currentForce = startingForce;

        if (player != null)
        {
            lastGroundY = player.transform.position.y;
            wasGrounded = true;
        }
    }
}

