using UnityEngine;

public class EscapeKey : MonoBehaviour
{
    [Header("Kaçış Noktaları (Local Offset)")]
    public Vector2 firstEscapeOffset = new Vector2(5, 2);  // İlk kaçacağı yer
    public Vector2 secondEscapeOffset = new Vector2(10, 0); // İkinci (son) kaçacağı yer

    [Header("Settings")]
    public float triggerDistance = 3.5f;
    public float moveSpeed = 10f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private int escapePhase = 0; // 0: Sabit, 1: İlk kaçış, 2: İkinci kaçış
    private bool canBeCollected = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos;
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);

        // KAÇIŞ TETİKLEME MANTIĞI
        if (dist < triggerDistance)
        {
            if (escapePhase == 0) 
            {
                escapePhase = 1;
                targetPos = startPos + (Vector3)firstEscapeOffset;
                Debug.Log("Anahtar: İlk durak! 🏃‍♂️");
            }
            else if (escapePhase == 1 && Vector3.Distance(transform.position, targetPos) < 0.2f)
            {
                // İlk noktaya vardı ve oyuncu hala kovalıyorsa ikinciye kaç
                escapePhase = 2;
                targetPos = startPos + (Vector3)secondEscapeOffset;
                Debug.Log("Anahtar: Burası çok kalabalık, ben kaçar! 💨");
            }
        }

       
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // SON DURAK KONTROLÜ
        if (escapePhase == 2 && Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            canBeCollected = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canBeCollected)
        {
            gameObject.SetActive(false);
            Debug.Log("Anahtar Sonunda Pes Etti!");
            if (GateController.Instance != null)
            {
                GateController.Instance.OpenGate();
            }
        }
    }
   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + (Vector3)firstEscapeOffset, 0.4f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + (Vector3)secondEscapeOffset, 0.4f);
    }
}