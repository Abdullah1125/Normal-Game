using UnityEngine;

/// <summary>
/// A custom gate that opens progressively. Disables the normal GateController when spawned,
/// and re-enables it when destroyed. Detaches and manages a movement particle effect.
/// (Aþamalý açýlan özel kapý. Normal kapýyý gizler, hareket efektini ayýrýr ve yönetir.)
/// </summary>
public class ProgressiveGateController : MonoBehaviour, IResettable
{
    [Header("Movement Settings (Hareket Ayarlarý)")]
    public Vector3 moveOffset = new Vector3(0, 3f, 0);
    public float moveSpeed = 5f;

    [Header("Drop Settings (Düþme Ayarlarý)")]
    [Tooltip("If > 0, the gate will slowly close if the player stops jumping. (0'dan büyükse zýplamayý býrakýnca kapý kapanýr.)")]
    public float fallDropRate = 0.5f;

    [Header("Visual Effects (Görsel Efektler)")]
    public ParticleSystem moveEffect; // Hareket sýrasýnda oynatýlacak efekt

    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _currentProgress = 0f;
    private bool _didDisableNormalGate = false;

    private void Awake()
    {
        _startPos = transform.position;
        _endPos = _startPos + moveOffset;

        // Efekti kapýdan ayýr (Yerde sabit kalmasý için)
        if (moveEffect != null)
        {
            moveEffect.transform.SetParent(null);
        }
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        // Normal kapýyý (Singleton) bul ve uykuya al
        if (GateController.Instance != null && GateController.Instance.gameObject.activeSelf)
        {
            GateController.Instance.gameObject.SetActive(false);
            _didDisableNormalGate = true;
            Debug.Log("Özel Kontrol: Normal kapý gizlendi, aþamalý kapý devrede.");
        }
    }

    private void Update()
    {
        // Troll Mekaniði: Oyuncu zýplamayý býrakýrsa kapý yavaþça geri kapanýr
        if (_currentProgress > 0f && _currentProgress < 1f && fallDropRate > 0f)
        {
            _currentProgress -= fallDropRate * Time.deltaTime;
            _currentProgress = Mathf.Clamp01(_currentProgress);
        }

        Vector3 currentTarget = Vector3.Lerp(_startPos, _endPos, _currentProgress);

        // --- EFEKT VE HAREKET MANTIÐI ---
        if (Vector3.Distance(transform.position, currentTarget) < 0.001f)
        {
            // Hedefe ulaþýldý: Pozisyonu sabitle ve efekti durdur
            transform.position = currentTarget;

            if (moveEffect != null && moveEffect.isPlaying)
            {
                moveEffect.Stop();
            }
        }
        else
        {
            // Hedefe gidiliyor: Hareketi saðla ve efekti oynat
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

            if (moveEffect != null && !moveEffect.isPlaying)
            {
                moveEffect.Play();
            }
        }
    }

    /// <summary>
    /// Adds progress to the gate to open it slightly.
    /// (Kapýyý bir miktar açmak için ilerleme ekler.)
    /// </summary>
    public void AddProgress(float amount)
    {
        _currentProgress += amount;
        _currentProgress = Mathf.Clamp01(_currentProgress); // %100'ü (1.0) geçmesini engeller
    }

    public void ResetMechanic()
    {
        _currentProgress = 0f;
        transform.position = _startPos;

        // Sýfýrlanýrken havada kalan tozu temizle
        if (moveEffect != null)
        {
            moveEffect.Stop();
            moveEffect.Clear();
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }

        // Level bitince / Obje silinince normal kapýyý geri uyandýr
        if (_didDisableNormalGate && GateController.Instance != null)
        {
            GateController.Instance.gameObject.SetActive(true);
        }

        // --- KESÝN ÇÖZÜM: Kapý silindiðinde sahnede çöp kalmamasý için baðýmsýz efekti de sil ---
        if (moveEffect != null && moveEffect.gameObject != null)
        {
            Destroy(moveEffect.gameObject);
        }
    }
}