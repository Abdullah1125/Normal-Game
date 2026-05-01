using UnityEngine;
using TMPro;

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

    // SES KORUMASI: Başlangıçta kapalı
    private bool _canPlaySound = false;

    protected override void Awake()
    {
        base.Awake();
        startPos = transform.position;
        targetPos = startPos + moveOffset;
        _canPlaySound = false; // Awake anında ses KESİNLİKLE yasak
    }

    private void Start()
    {
        UpdateKeyUI();
        if (LevelManager.Instance != null) LevelManager.Instance.RegisterResettable(this);

        // TÜM FİZİKLERİN OTURMASI İÇİN 0.5 SANİYE BEKLE (En güvenli süre)
        Invoke(nameof(EnableSound), 0.5f);
    }

    private void EnableSound() => _canPlaySound = true;

    void Update()
    {
        Vector3 currentTarget = isOpening ? targetPos : startPos;
        if (Vector3.Distance(transform.position, currentTarget) < 0.001f)
        {
            transform.position = currentTarget;
            if (frictionParticles != null && frictionParticles.isPlaying) frictionParticles.Stop();
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
        if (frictionParticles != null && !frictionParticles.isPlaying) frictionParticles.Play();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(Constants.TAG_PLAYER))
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

            // KESİN ÇÖZÜM: Eğer LevelManager şu an bir şeyleri siliyor veya yüklüyorsa SUS!
            if (LevelManager.IsTransitioning) return;

            if (_canPlaySound && gameObject.scene.isLoaded && gameObject.activeInHierarchy)
            {
                SoundManager.PlayThemeSFX(SFXType.SlidingDoor);
            }
        }
    }

    public void RegisterKeyCollected() { keysCollected++; UpdateKeyUI(); if (keysCollected >= totalKeysNeeded) allKeysCollected = true; }

    public void ResetMechanic()
    {
        _canPlaySound = false; // Resetlendiğinde sesi anında kapat
        keysCollected = 0;
        allKeysCollected = false;
        isOpening = false;
        transform.position = startPos;
        UpdateKeyUI();
        Invoke(nameof(EnableSound), 0.5f); // Yarım saniye sonra tekrar aç
    }

    public void UpdateKeyUI() { if (keyCountText != null) keyCountText.text = keysCollected + " / " + totalKeysNeeded; }
    private void OnDestroy() { if (LevelManager.Instance != null) LevelManager.Instance.UnregisterResettable(this); }
}