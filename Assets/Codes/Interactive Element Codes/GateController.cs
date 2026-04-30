using UnityEngine;
using TMPro;

public class GateController : Singleton<GateController> , IResettable
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


    [Header("Effects (Efektler)")]
    public ParticleSystem frictionParticles; // SÃ¼rgÃ¼lÃ¼ kapÄ± sÃ¼rtÃ¼nme tozu
    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        targetPos = startPos + moveOffset;
       
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

        // SÄ°HÄ°R 1: EÄŸer kapÄ± zaten hedefe Ã§ok yakÄ±nsa Update'i yorma, direkt oraya sabitle
        if (Vector3.Distance(transform.position, currentTarget) < 0.001f)
        {
            transform.position = currentTarget;
            if (frictionParticles != null && frictionParticles.isPlaying) frictionParticles.Stop();
            return;
        }

        // Hareket kodu
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

        // SÄ°HÄ°R 2: PartikÃ¼lleri sadece hareket varsa ve Ã§almÄ±yorsa baÅŸlat
        if (frictionParticles != null && !frictionParticles.isPlaying)
        {
            frictionParticles.Play();
        }
    }

    // --- DEÄÄ°NCE AÃ‡ILMA VE AKTÄ°FLÄ°K KONTROLÃœ ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("KapÄ±ya bir ÅŸey Ã§arptÄ±: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
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

            //Sahne hala yÃ¼klÃ¼yse ve obje silinmiyorsa ses Ã§Ä±kar
            if (gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
            }
        }
    }


    public void CloseGate()
    {
        // KapÄ± zaten kapalÄ±ysa hiÃ§bir ÅŸey yapma 
        if (isOpening)
        {
            isOpening = false;

            // Sahne silinirken (fiÅŸ Ã§ekilirken) o sahte sesi yaratma!
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
        // Obje silinirken LevelManager'Ä±n listesini de temizliyoruz
        if (LevelManager.Instance != null)
        {
            // EÄŸer LevelManager'da RemoveResettable fonksiyonu yoksa aÅŸaÄŸÄ±ya ekledim
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}

