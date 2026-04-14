using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour
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

    void Awake()
    {
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        if (Instance == null) Instance = this;
       
    }
    private void Start()
    {
        UpdateKeyUI();
    }

    void Update()
    {
        Vector3 currentTarget = isOpening ? targetPos : startPos;
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
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
            SoundManager.PlaySFX(SoundManager.instance.slidingDoorSound);
        }
    }


    public void CloseGate()
    {
        isOpening = false;
        SoundManager.PlaySFX(SoundManager.instance.slidingDoorSound);
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

    public void ResetGate()
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
}