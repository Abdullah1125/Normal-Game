using UnityEngine;

public class GateController : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(0, 3f, 0); // Kapýnýn ne kadar kayacaðýný belirleyen mesafe
    public float moveSpeed = 2f;                       // Kapýnýn açýlma hýzý
    private Vector3 startPos;                          // Kapýnýn baþlangýç konumu
    private Vector3 targetPos;                         // Kapýnýn hedef konumu
    private bool isOpening = false;                    // Kapý þu an açýlýyor mu kontrolü
    public static GateController Instance;

    void Awake()
    {

        startPos = transform.position;
        targetPos = startPos + moveOffset;
        if (Instance == null) Instance = this;
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
        isOpening = false;
        transform.position = startPos;
    }
}