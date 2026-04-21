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
    /// Plays a theme-specific sound effect using a temporary audio source to prevent pitch overlapping.
    /// (Pitch çakýþmasýný önlemek için geçici bir ses kaynaðý kullanarak temaya özel ses efektini çalar.)
    /// </summary>
    public static void PlayThemeSFX(SFXType type)
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
            PlayClipWithPitch(clipToPlay);
        }
    }

    /// <summary>
    /// Plays an external audio clip using a temporary audio source.
    /// (Harici bir ses dosyasýný geçici bir ses kaynaðý ile çalar.)
    /// </summary>
    public static void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            PlayClipWithPitch(clip);
        }
    }

    /// <summary>
    /// Creates a temporary AudioSource to play the clip with a random pitch, then destroys it.
    /// (Sesi rastgele bir pitch ile çalmak için geçici bir AudioSource oluþturur, sonra yok eder.)
    /// </summary>
    private static void PlayClipWithPitch(AudioClip clip)
    {
        // Geçici bir hayalet obje oluþturuyoruz
        GameObject tempAudioObj = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();

        // Ana hoparlördeki ayarlarý (Ses seviyesi, Mixer ayarý) kopyalýyoruz
        if (instance != null && instance.sfxSource != null)
        {
            tempSource.volume = instance.sfxSource.volume;
            tempSource.outputAudioMixerGroup = instance.sfxSource.outputAudioMixerGroup;
        }

        // Sesi ve rastgele pitch ayarýný yüklüyoruz
        tempSource.clip = clip;
        tempSource.pitch = Random.Range(0.9f, 1.05f); // Biraz daha esneklik kattým

        // Sesi patlat
        tempSource.Play();

        // Saniyesi saniyesine ses bittiði an objeyi sahneden sil (çöp biriktirme)
        Destroy(tempAudioObj, clip.length / tempSource.pitch);
    }
}