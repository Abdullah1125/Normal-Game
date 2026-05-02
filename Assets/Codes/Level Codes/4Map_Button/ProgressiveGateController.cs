using UnityEngine;

/// <summary>
/// A custom gate that opens progressively. Disables the normal GateController when spawned,
/// and re-enables it when destroyed (level passed).
/// (Aþamalý açýlan özel kapý. Sahneye gelince normal kapýyý gizler, yok olurken geri açar.)
/// </summary>
public class ProgressiveGateController : MonoBehaviour, IResettable
{
    [Header("Movement Settings (Hareket Ayarlarý)")]
    public Vector3 moveOffset = new Vector3(0, 3f, 0);
    public float moveSpeed = 5f;

    [Header("Troll Settings (Troll Ayarlarý)")]
    [Tooltip("If > 0, the gate will slowly close if the player stops jumping. (0'dan büyükse zýplamayý býrakýnca kapý kapanýr.)")]
    public float fallDropRate = 0.5f;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _currentProgress = 0f;
    private bool _didDisableNormalGate = false;

    private void Awake()
    {
        _startPos = transform.position;
        _endPos = _startPos + moveOffset;
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }

        //Normal kapýyý (Singleton) bul ve uykuya al ---
        if (GateController.Instance != null && GateController.Instance.gameObject.activeSelf)
        {
            GateController.Instance.gameObject.SetActive(false);
            _didDisableNormalGate = true;
            Debug.Log("JÝLET TROLL: Normal kapý gizlendi, özel kapý devrede.");
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

        // Kapýnýn o anki hedef noktasýný hesapla ve yumuþakça hareket ettir
        Vector3 currentTarget = Vector3.Lerp(_startPos, _endPos, _currentProgress);
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
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
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }

        // --- ALTIN VURUÞ: Level bitince / Obje silinince normal kapýyý geri uyandýr ---
        if (_didDisableNormalGate && GateController.Instance != null)
        {
            GateController.Instance.gameObject.SetActive(true);
            Debug.Log(": Özel kapý silindi, normal kapý geri açýldý.");
        }
    }
}