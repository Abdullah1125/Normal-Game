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
    SlidingDoor
}

/// <summary>
/// Data class holding specific audio clips for a theme.
/// (Bir temaya ait özel ses dosyalarýný tutan veri sýnýfý.)
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
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Speaker (Hoparlörler)")]
    public AudioSource sfxSource;

    [Header("Theme Packages (Tema Ses Paketleri)")]
    public ThemeAudio[] themeAudios = new ThemeAudio[5];

    private int currentThemeIndex = 0;
    private static System.Collections.Generic.Dictionary<AudioClip, float> _soundTimers = new System.Collections.Generic.Dictionary<AudioClip, float>();
    /// <summary>
    /// Sets up the singleton pattern.
    /// (Singleton yapýsýný kurar.)
    /// </summary>
    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Updates the current audio theme based on the active level ID.
    /// (Aktif bölüm ID'sine göre mevcut ses temasýný günceller.)
    /// </summary>
    public static void UpdateThemeByLevelID(int levelID)
    {
        if (instance == null || instance.themeAudios.Length == 0) return;

        instance.currentThemeIndex = levelID / 12;
        instance.currentThemeIndex = Mathf.Clamp(instance.currentThemeIndex, 0, instance.themeAudios.Length - 1);
    }

    /// <summary>
    /// Plays a theme-specific sound effect. Adds a volume parameter to lower specific loud sounds.
    /// (Belirli çok çýkan sesleri kýsmak için ses seviyesi parametresi eklendi.)
    /// </summary>
    public static void PlayThemeSFX(SFXType type, float volumeMultiplier = 1f)
    {
        if (instance == null || instance.themeAudios.Length == 0) return;

        ThemeAudio currentTheme = instance.themeAudios[instance.currentThemeIndex];
        AudioClip clipToPlay = null;

        switch (type)
        {
            case SFXType.Jump: clipToPlay = currentTheme.jumpSound; break;
            case SFXType.Die: clipToPlay = currentTheme.dieSound; break;
            case SFXType.Button: clipToPlay = currentTheme.buttonSound; break;
            case SFXType.Key: clipToPlay = currentTheme.keySound; break;
            case SFXType.DoorPass: clipToPlay = currentTheme.doorPassSound; break;
            case SFXType.SlidingDoor: clipToPlay = currentTheme.slidingDoorSound; break;
        }

        if (clipToPlay != null)
        {
            PlayClipWithPitch(clipToPlay, volumeMultiplier);
        }
    }
    // <summary>
    /// Creates a temporary AudioSource, adjusts volume, and prevents overlapping spam.
    /// (Geçici bir AudioSource oluþturur, sesi ayarlar ve üst üste binme spam'ini önler.)
    /// </summary>
    private static void PlayClipWithPitch(AudioClip clip, float volumeMultiplier)
    {
        // 1. ANTI-SPAM KONTROLÜ (Ayný ses 0.08 saniye içinde tekrar çalamaz)
        if (_soundTimers.TryGetValue(clip, out float lastPlayedTime))
        {
            // Eðer son çalýnma üzerinden 0.08 saniyeden az zaman geçtiyse, iptal et!
            if (Time.unscaledTime - lastPlayedTime < 0.08f) return;
        }

        // Sesin son çalýnma zamanýný hafýzaya kaydet
        _soundTimers[clip] = Time.unscaledTime;

        // 2. HAYALET OBJE YARATMA (Eski kodun aynýsý)
        GameObject tempAudioObj = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();

        if (instance != null && instance.sfxSource != null)
        {
            tempSource.volume = instance.sfxSource.volume * volumeMultiplier;
            tempSource.outputAudioMixerGroup = instance.sfxSource.outputAudioMixerGroup;
        }

        tempSource.clip = clip;
        tempSource.pitch = Random.Range(0.9f, 1.05f);

        tempSource.Play();
        Destroy(tempAudioObj, clip.length / tempSource.pitch);
    }
}