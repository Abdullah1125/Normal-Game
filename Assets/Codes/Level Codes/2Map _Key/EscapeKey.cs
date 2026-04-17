using UnityEngine;

public class EscapeKey : MonoBehaviour
{
    [Header("Kaçış Noktaları (Local Offset)")]
    public Vector2 firstEscapeOffset = new Vector2(5, 2);
    public Vector2 secondEscapeOffset = new Vector2(10, 0);

    [Header("Settings(Ayarlar")]
    public float triggerDistance = 3.5f;
    public float moveSpeed = 10f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private int escapePhase = 0;
    private Collider2D myCollider; // Kendi collider'ımız

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos;
        myCollider = GetComponent<Collider2D>();

       
    }

    void Update()
    {
        if (PlayerController.Instance == null) return;
        float dist = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);

        // 1. KAÇIŞ TETİKLEME
        if (dist < triggerDistance)
        {
            if (escapePhase == 0)
            {
                escapePhase = 1;
                targetPos = startPos + (Vector3)firstEscapeOffset;
            }
            else if (escapePhase == 1 && Vector3.Distance(transform.position, targetPos) < 0.2f)
            {
                escapePhase = 2;
                targetPos = startPos + (Vector3)secondEscapeOffset;
            }
        }

        // 2. HAREKET
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // 3. COLLIDER KONTROLÜ (Burası mermi!)
        if (escapePhase == 2 && Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            // SADECE son durakta ve durmuşken collider aktif olur
            if (myCollider != null) myCollider.enabled = true;
        }
        else if (escapePhase > 0)
        {
            // Anahtar kaçmaya başladığı an collider kapanır (HAYALET MODU)
            if (myCollider != null) myCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GateController.Instance != null)
            {
                GateController.Instance.RegisterKeyCollected();
                if (SoundManager.instance != null) SoundManager.PlaySFX(SoundManager.instance.keySound);
                gameObject.SetActive(false);
            }
        }
    }

    public void ResetKey()
    {
        gameObject.SetActive(true);
        transform.position = startPos;
        targetPos = startPos;
        escapePhase = 0;
        if (myCollider != null) myCollider.enabled = true; // Reset atınca tekrar aç
    }
}