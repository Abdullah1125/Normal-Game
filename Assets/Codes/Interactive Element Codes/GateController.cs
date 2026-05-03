using UnityEngine;
using TMPro;

/// <summary>
/// Manages the gate state, movement, and visual/audio feedback.
/// Handles detached particle lifecycle.
/// (Kapı durumunu, hareketini ve görsel/işitsel geri bildirimleri yönetir. Bağımsız parçacık yaşam döngüsünü kontrol eder.)
/// </summary>
public class GateController : Singleton<GateController>, IResettable
{
    public Vector3 moveOffset = new Vector3(0, 3f, 0);
    public float moveSpeed = 2f;
    public int totalKeysNeeded = 2;
    public TextMeshProUGUI keyCountText;
    public ParticleSystem frictionParticles;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool isOpening = false;
    private bool allKeysCollected = false;
    private int keysCollected = 0;

    private bool _canPlaySound = false;

    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        _canPlaySound = false;

        // Efekti kapıdan ayır (Yerde sabit kalması için)
        if (frictionParticles != null)
        {
            frictionParticles.transform.SetParent(null);
        }
    }

    // --- YAŞAM DÖNGÜSÜ YÖNETİMİ (LIFECYCLE MANAGEMENT) ---

    /// <summary>
    /// Enables the particle system when the gate is enabled.
    /// (Kapı aktifleştirildiğinde parçacık sistemini de aktifleştirir.)
    /// </summary>
    private void OnEnable()
    {
        if (frictionParticles != null)
        {
            frictionParticles.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Disables the particle system when the gate is disabled.
    /// (Kapı devre dışı bırakıldığında parçacık sistemini de kapatır.)
    /// </summary>
    private void OnDisable()
    {
        if (frictionParticles != null)
        {
            frictionParticles.Stop();
            frictionParticles.Clear();
            frictionParticles.gameObject.SetActive(false); // Kapı gizlenince efekt de gizlenir
        }
    }

    private void Start()
    {
        UpdateKeyUI();
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterResettable(this);

        Invoke(nameof(EnableSound), 0.5f);
    }

    private void EnableSound() => _canPlaySound = true;

    void Update()
    {
        Vector3 currentTarget = isOpening ? targetPos : startPos;

        if (Vector3.Distance(transform.position, currentTarget) < 0.001f)
        {
            transform.position = currentTarget;

            if (frictionParticles != null && frictionParticles.isPlaying)
            {
                frictionParticles.Stop();
            }
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);

        if (frictionParticles != null && !frictionParticles.isPlaying)
        {
            frictionParticles.Play();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
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
            if (LevelManager.IsTransitioning) return;

            if (_canPlaySound && gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
            }
        }
    }

    public void CloseGate()
    {
        if (isOpening)
        {
            isOpening = false;
            if (LevelManager.IsTransitioning) return;

            if (_canPlaySound && gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
            }
        }
    }

    public void RegisterKeyCollected()
    {
        keysCollected++;
        UpdateKeyUI();
        if (keysCollected >= totalKeysNeeded) allKeysCollected = true;
    }

    public void ResetMechanic()
    {
        _canPlaySound = false;
        keysCollected = 0;
        allKeysCollected = false;
        isOpening = false;
        transform.position = startPos;
        UpdateKeyUI();

        if (frictionParticles != null)
        {
            frictionParticles.Stop();
            frictionParticles.Clear();
        }

        Invoke(nameof(EnableSound), 0.5f);
    }

    public void UpdateKeyUI()
    {
        if (keyCountText != null) keyCountText.text = keysCollected + " / " + totalKeysNeeded;
    }

    /// <summary>
    /// Cleans up memory by destroying the detached particle system.
    /// (Bağımsız parçacık sistemini yok ederek belleği temizler.)
    /// </summary>
    private void OnDestroy()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.UnregisterResettable(this);

        // Kapı tamamen silindiğinde, sahnede çöp kalmaması için efekti de kalıcı olarak sil
        if (frictionParticles != null && frictionParticles.gameObject != null)
        {
            Destroy(frictionParticles.gameObject);
        }
    }
}