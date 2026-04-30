using UnityEngine;

public class MapWindManager : MonoBehaviour
{
    [Header("Wind Power (Rüzgar Gücü)")]
    public Vector2 windForce = new Vector2(-20f, 0f);
    public bool isWindActive = true;

    [Header("Speed Settings (Hýz Ayarlarý)")]
    public float windSpeed = 35f;
    private float normalSpeed;

    [Header("Visual & Audio Effects (Efekt ve Ses Ayarlarý)")]
    public ParticleSystem windParticles;
    public AudioSource windAudio;

    private PlayerController playerScript;
    private Rigidbody2D playerRb;

    private void OnEnable() => LevelManager.OnLevelStarted += ReApplyWindEffect;
    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ReApplyWindEffect;
        if (playerScript != null) playerScript.moveSpeed = normalSpeed;
        ToggleEffects(false);
    }

    void Start()
    {
        if (PlayerController.Instance != null)
        {
            playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
            playerScript = PlayerController.Instance.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                normalSpeed = PlayerController.Instance.defaultSpeed;
                ApplyWindEffect();
            }
        }
    }

    /// <summary>
    /// Handles pause/resume states for audio.
    /// (Ses için duraklatma ve devam ettirme durumlarýný yönetir.)
    /// </summary>
    void Update()
    {
        if (windAudio == null) return;

        // Oyun durmuþsa sesi duraklat
        if (Time.timeScale == 0f && windAudio.isPlaying)
        {
            windAudio.Pause();
        }
        // Oyun devam ediyorsa ve rüzgar aktifse sesi sürdür
        else if (Time.timeScale > 0f && isWindActive && !windAudio.isPlaying)
        {
            windAudio.UnPause();
        }
    }

    void FixedUpdate()
    {
        if (isWindActive && playerRb != null)
        {
            playerRb.AddForce(windForce, ForceMode2D.Force);
        }
    }

    /// <summary>
    /// Adjusts speed and triggers effects.
    /// (Hýzý ayarlar ve efektleri tetikler.)
    /// </summary>
    void ApplyWindEffect()
    {
        if (playerScript == null) return;

        if (isWindActive)
        {
            playerScript.moveSpeed = windSpeed;
            ToggleEffects(true);
        }
        else
        {
            playerScript.moveSpeed = normalSpeed;
            ToggleEffects(false);
        }
    }

    private void ReApplyWindEffect()
    {
        if (this.gameObject.activeInHierarchy) ApplyWindEffect();
    }

    /// <summary>
    /// Toggles VFX and SFX.
    /// (Görsel ve ses efektlerini açýp kapatýr.)
    /// </summary>
    private void ToggleEffects(bool turnOn)
    {
        if (turnOn)
        {
            if (windParticles != null && !windParticles.isPlaying) windParticles.Play();

            if (windAudio != null)
            {
                windAudio.volume = 0.4f;
                windAudio.loop = true;
                // Sadece oyun durmamýþsa çalmaya baþla
                if (!windAudio.isPlaying && Time.timeScale > 0f) windAudio.Play();
            }
        }
        else
        {
            if (windParticles != null) windParticles.Stop();
            if (windAudio != null) windAudio.Stop();
        }
    }
}