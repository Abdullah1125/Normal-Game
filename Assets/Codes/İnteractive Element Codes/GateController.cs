using UnityEngine;
using TMPro;

public class GateController : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(0, 3f, 0); // Kapýnýn ne kadar kayacaðýný belirleyen mesafe
    public float moveSpeed = 2f;                       // Kapýnýn açýlma hýzý
    private Vector3 startPos;                          // Kapýnýn baþlangýç konumu
    private Vector3 targetPos;                         // Kapýnýn hedef konumu
    private bool isOpening = false;                    // Kapý þu an açýlýyor mu kontrolü
    public int totalKeysNeeded = 2; // Bu bölmede kaç anahtar lazým? 
    private int keysCollected = 0;
    public TextMeshProUGUI keyCountText;
    public static GateController Instance;

    void Awake()
    {

        startPos = transform.position;
        targetPos = startPos + moveOffset;
        if (Instance == null) Instance = this;
        UpdateKeyUI();
    }

    void Update()
    {
        Vector3 currentTarget = isOpening ? targetPos : startPos;

        // MoveTowards her iki yön için de otomatik çalýþýr
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
    }
    public void OpenGate() => isOpening = true;
    public void CloseGate() => isOpening = false;
       
    // Kapýyý baþlangýç durumuna döndüren metot
    public void ResetGate()
    {
        keysCollected = 0;
        isOpening = false;
        transform.position = startPos;
    }
    public void RegisterKeyCollected()
    {
        keysCollected++;
        UpdateKeyUI();
        Debug.Log("Anahtar toplandý: " + keysCollected + "/" + totalKeysNeeded);
        if (keysCollected >= totalKeysNeeded)
        {
            OpenGate();
        }
    }

    public void UpdateKeyUI()
    {
        if (keyCountText != null)
        {
            keyCountText.text = keysCollected + " / " + totalKeysNeeded;
        }
    }
}