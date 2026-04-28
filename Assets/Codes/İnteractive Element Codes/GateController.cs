using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour , IResettable
{
    public Vector3 moveOffset = new Vector3(0, 3f, 0);
    public float moveSpeed = 2f;
    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isOpening = false;
    private bool allKeysCollected = false;

    public int totalKeysNeeded = 2;
    private int keysCollected = 0;
    public TextMeshProUGUI keyCountText;
    public static GateController Instance;

    [Header("Effects (Efektler)")]
    public ParticleSystem frictionParticles; // Sürgülü kapý sürtünme tozu
    void Awake()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        if (Instance == null) Instance = this;
       
    }
    private void Start()
    {
        UpdateKeyUI();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    void Update()
    {
        Vector3 currentTarget = isOpening ? targetPos : startPos;

        // SÝHÝR 1: Eðer kapý zaten hedefe çok yakýnsa Update'i yorma, direkt oraya sabitle
        if (Vector3.Distance(transform.position, currentTarget) < 0.001f)
        {
            transform.position = currentTarget;
            if (frictionParticles != null && frictionParticles.isPlaying) frictionParticles.Stop();
            return;
        }

        // Hareket kodu
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

        // SÝHÝR 2: Partikülleri sadece hareket varsa ve çalmýyorsa baþlat
        if (frictionParticles != null && !frictionParticles.isPlaying)
        {
            frictionParticles.Play();
        }
    }

    // --- DEÐÝNCE AÇILMA VE AKTÝFLÝK KONTROLÜ ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Kapýya bir þey çarptý: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Gelen oyuncu! Anahtar durumu: " + keysCollected + " / " + totalKeysNeeded);

            if (!allKeysCollected) return;

            if (LevelManager.Instance != null && !LevelManager.Instance.activeLevel.isActive) return;
            

            OpenGate();
        }
    }

    public void OpenGate()
    {
        if (!isOpening)
        {
            isOpening = true;

            //Sahne hala yüklüyse ve obje silinmiyorsa ses çýkar
            if (gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
            }
        }
    }


    public void CloseGate()
    {
        // Kapý zaten kapalýysa hiçbir þey yapma 
        if (isOpening)
        {
            isOpening = false;

            // Sahne silinirken (fiþ çekilirken) o sahte sesi yaratma!
            if (gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
            }
        }
    }

    public void RegisterKeyCollected()
    {
        keysCollected++;
        UpdateKeyUI();

        if (keysCollected >= totalKeysNeeded)
        {
            allKeysCollected = true;
        }
    }

    public void ResetMechanic()
    {
        keysCollected = 0;
        allKeysCollected = false;
        isOpening = false;
        transform.position = startPos;
        UpdateKeyUI();
    }

    public void UpdateKeyUI()
    {
        if (keyCountText != null)
        {
            keyCountText.text = keysCollected + " / " + totalKeysNeeded;
        }
    }

    private void OnDestroy()
    {
        // Obje silinirken LevelManager'ýn listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // Eðer LevelManager'da RemoveResettable fonksiyonu yoksa aþaðýya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    
        if (Instance == this)
        {
            Instance = null;
        }

    }
}