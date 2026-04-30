using UnityEngine;

/// <summary>
/// List of all sound effect types.
/// (Tüm ses efekti türlerinin listesi.)
/// </summary>
public enum SFXType
{
    Jump,
    Die,
    Button,
    Key,
    DoorPass,
    SlidingDoor,
    MenuPop,   // Ortadan açılan için
    MenuSlide
}

/// <summary>
/// Data class holding specific audio clips for a theme.
/// (Bir temaya ait özel ses dosyalarını tutan veri sınıfı.)
/// </summary>
[System.Serializable]
public class ThemeAudio
{
    public string themeName = "New Theme";

    public AudioClip jumpSound;
    public AudioClip dieSound;
    public AudioClip buttonSound;
    public AudioClip keySound;
    public AudioClip doorPassSound;
    public AudioClip slidingDoorSound;
    public AudioClip menuPopSound;
    public AudioClip MenuSlide;


}

public class SoundManager : SingletonPersistent<SoundManager>
{

    [Header("Speaker (Hoparlörler)")]
    public AudioSource sfxSource;

    [Header("Theme Packages (Tema Ses Paketleri)")]
    public ThemeAudio[] themeAudios = new ThemeAudio[5];

    private int currentThemeIndex = 0;
    private static System.Collections.Generic.Dictionary<AudioClip, float> _soundTimers = new System.Collections.Generic.Dictionary<AudioClip, float>();
    /// <summary>
    /// Sets up the singleton pattern.
    /// (Singleton yapısını kurar.)
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Updates the current audio theme based on the active level ID.
    /// (Aktif bölüm ID'sine göre mevcut ses temasını günceller.)
    /// </summary>
    public static void UpdateThemeByLevelID(int levelID)
    {
        if (Instance == null || Instance.themeAudios.Length == 0) return;

        Instance.currentThemeIndex = levelID / 12;
        Instance.currentThemeIndex = Mathf.Clamp(Instance.currentThemeIndex, 0, Instance.themeAudios.Length - 1);
    }

    /// <summary>
    /// Plays a theme-specific sound effect. Adds a volume parameter to lower specific loud sounds.
    /// (Belirli çok çıkan sesleri kısmak için ses seviyesi parametresi eklendi.)
    /// </summary>
    public static void PlayThemeSFX(SFXType type, float volumeMultiplier = 1f)
    {
        if (Instance == null || Instance.themeAudios.Length == 0) return;

        ThemeAudio currentTheme = Instance.themeAudios[Instance.currentThemeIndex];
        AudioClip clipToPlay = null;

        switch (type)
        {
            case SFXType.Jump: clipToPlay = currentTheme.jumpSound; break;
            case SFXType.Die: clipToPlay = currentTheme.dieSound; break;
            case SFXType.Button: clipToPlay = currentTheme.buttonSound; break;
            case SFXType.Key: clipToPlay = currentTheme.keySound; break;
            case SFXType.DoorPass: clipToPlay = currentTheme.doorPassSound; break;
            case SFXType.SlidingDoor: clipToPlay = currentTheme.slidingDoorSound; break;
            case SFXType.MenuPop: clipToPlay = currentTheme.menuPopSound; break;
            case SFXType.MenuSlide: clipToPlay = currentTheme.MenuSlide; break;
        }

        if (clipToPlay != null)
        {
            PlayClipWithPitch(clipToPlay, volumeMultiplier);
        }
    }
    // <summary>
    /// Creates a temporary AudioSource, adjusts volume, and prevents overlapping spam.
    /// (Geçici bir AudioSource oluşturur, sesi ayarlar ve üst üste binme spam'ini önler.)
    /// </summary>
    private static void PlayClipWithPitch(AudioClip clip, float volumeMultiplier)
    {
        // 1. ANTI-SPAM KONTROLÜ (Aynı ses 0.08 saniye içinde tekrar çalamaz)
        if (_soundTimers.TryGetValue(clip, out float lastPlayedTime))
        {
            // Eğer son çalınma üzerinden 0.08 saniyeden az zaman geçtiyse, iptal et!
            if (Time.unscaledTime - lastPlayedTime < 0.08f) return;
        }

        // Sesin son çalınma zamanını hafızaya kaydet
        _soundTimers[clip] = Time.unscaledTime;

        // 2. HAYALET OBJE YARATMA (Eski kodun aynısı)
        GameObject tempAudioObj = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();

        if (Instance != null && Instance.sfxSource != null)
        {
            tempSource.volume = Instance.sfxSource.volume * volumeMultiplier;
            tempSource.outputAudioMixerGroup = Instance.sfxSource.outputAudioMixerGroup;
        }

        tempSource.clip = clip;
        tempSource.pitch = Random.Range(0.9f, 1.05f);

        tempSource.Play();
        Destroy(tempAudioObj, clip.length / tempSource.pitch);
    }
}
