using UnityEngine;

public class MapWindManager : MonoBehaviour
{

    [Header("Wind Power(Rüzgar Gücü)")]
    public Vector2 windForce = new Vector2(-20f, 0f);
    public bool isWindActive = true;

    [Header("Speed Settings(Hýz Ayarlarý)")]
    public float windSpeed = 35f;    // Rüzgar varkenki hýzý (Normali 10 ise bu 4 olsun)
    private float normalSpeed;          // Karakterin orijinal hýzý (Otomatik kaydedilir)

    private PlayerController playerScript;
    private Rigidbody2D playerRb;

    // Anons sistemine kayýt ol (Karakter her dirildiðinde burasý dinler)
    private void OnEnable()
    {
        LevelManager.OnLevelStarted += ReApplyWindEffect;
    }

  
    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ReApplyWindEffect;

        if (playerScript != null)
        {
            playerScript.moveSpeed = normalSpeed;
        }
    }

    void Start()
    {
        // Senin istediðin Tag kontrolü ile karakteri ve scriptini bul

        if (PlayerController.Instance != null)
        {
            playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
            playerScript = PlayerController.Instance.GetComponent<PlayerController>();

            // Karakterin orijinal hýzýný (moveSpeed) hafýzaya al
            if (playerScript != null)
            {
                normalSpeed = PlayerController.Instance.defaultSpeed;
                ApplyWindEffect(); // Oyun baþlarken rüzgar varsa hýzý düþür
            }
        }
    }

    void FixedUpdate()
    {
        // Sadece itiþ kuvvetini uygula (Hýz zaten moveSpeed üzerinden kýsýtlý)
        if (isWindActive && playerRb != null)
        {
            playerRb.AddForce(windForce, ForceMode2D.Force);
        }
    }

    // Rüzgarýn hýza etkisini uygula
    void ApplyWindEffect()
    {
        if (playerScript == null) return;

        if (isWindActive)
            playerScript.moveSpeed = windSpeed; // Hýzý düþür
        else
            playerScript.moveSpeed = normalSpeed;   // Eski haline getir
    }

    
    private void ReApplyWindEffect()
    {
        // Eðer bu rüzgar objesi hala o anki sahnede aktifse rüzgarý tekrar ver
        if (this.gameObject.activeInHierarchy)
        {
            ApplyWindEffect();
        }
    }
}