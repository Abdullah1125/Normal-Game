using UnityEngine;

public class MapWindManager : MonoBehaviour
{


    [Header("Rüzgar Gücü")]
    public Vector2 windForce = new Vector2(-20f, 0f);
    public bool isWindActive = true;

    [Header("Hýz Ayarlarý")]
    public float ruzgarliHiz = 4f;    // Rüzgar varkenki hýzý (Normali 10 ise bu 4 olsun)
    private float normalHiz;          // Karakterin orijinal hýzý (Otomatik kaydedilir)

    private PlayerController playerScript;
    private Rigidbody2D playerRb;


    void Start()
    {
        // Senin istediðin Tag kontrolü ile karakteri ve scriptini bul
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            playerScript = playerObj.GetComponent<PlayerController>();

            // Karakterin orijinal hýzýný (moveSpeed) hafýzaya al
            if (playerScript != null)
            {
                normalHiz = playerScript.moveSpeed;
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
            playerScript.moveSpeed = ruzgarliHiz; // Hýzý düþür
        else
            playerScript.moveSpeed = normalHiz;   // Eski haline getir
    }

}