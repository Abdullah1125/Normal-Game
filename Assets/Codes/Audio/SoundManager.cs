using UnityEngine;
using System.Collections.Generic;

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
    MenuPop,
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

    [Header("Pool Settings (Havuz Ayarları)")]
    public int initialPoolSize = 10;
    public int maxPoolSize = 20; // Havuzun çıkabileceği maksimum sınır!

    private int currentThemeIndex = 0;
    private static Dictionary<AudioClip, float> _soundTimers = new Dictionary<AudioClip, float>();
    
    //  Ses objelerini tutacağımız havuz listesi
    private List<AudioSource> audioPool = new List<AudioSource>();

    /// <summary>
    /// Sets up the singleton pattern and initializes the audio pool.
    /// (Singleton yapısını kurar ve ses havuzunu başlatır.)
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

        InitializePool();
    }

    /// <summary>
    /// Creates the initial pool of AudioSources to prevent GC allocation at runtime.
    /// (Çalışma zamanında GC yükünü önlemek için başlangıç AudioSource havuzunu oluşturur.)
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    /// <summary>
    /// Creates a new AudioSource child object and adds it to the pool.
    /// (Yeni bir AudioSource alt objesi oluşturur ve havuza ekler.)
    /// </summary>
    private AudioSource CreateNewAudioSource()
    {
        GameObject obj = new GameObject("PooledSFX_" + audioPool.Count);
        obj.transform.SetParent(transform); // Hiyerarşiyi temiz tutmak için SoundManager'ın altına atar
        AudioSource newSource = obj.AddComponent<AudioSource>();

        if (sfxSource != null)
        {
            newSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }

        audioPool.Add(newSource);
        return newSource;
    }

   /// <summary>
    /// Havuzdan boş bir kaynak bulur. Boş yoksa ve sınırı aşmadıysa yeni üretir.
    /// Sınır aşıldıysa en eski çalan sesi zorla susturup onu verir (Voice Stealing).
    /// </summary>
    private AudioSource GetAvailableSource()
    {
        AudioSource oldestSource = null;
        float oldestTime = float.MaxValue;

        // 1. Havuzda boşta yatan bir hoparlör var mı diye bak
        for (int i = 0; i < audioPool.Count; i++)
        {
            if (!audioPool[i].isPlaying)
            {
                return audioPool[i];
            }

            // Çalanlar arasında en eskisini bul (Voice Stealing için yedekte tutuyoruz)
            if (audioPool[i].time < oldestTime)
            {
                oldestTime = audioPool[i].time;
                oldestSource = audioPool[i];
            }
        }

        // 2. Havuzda boş yok ama MAKSİMUM sınıra (20) henüz ulaşmadıysak, yeni üret.
        if (audioPool.Count < maxPoolSize)
        {
            return CreateNewAudioSource();
        }

        // 3. MAKSİMUM sınıra (20) ulaştıysak ve hepsi çalıyorsa, EN ESKİSİNİ SUSTUR VE ÇAL! (Voice Stealing)
        oldestSource.Stop();
        return oldestSource;
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
    /// Plays a theme-specific sound effect.
    /// (Temaya özel bir ses efekti çalar.)
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

    /// <summary>
    /// Plays a clip using pooled AudioSources to prevent memory allocation spam.
    /// (Bellek şişmesini önlemek için havuzlanmış AudioSource'ları kullanarak ses çalar.)
    /// </summary>
    private static void PlayClipWithPitch(AudioClip clip, float volumeMultiplier)
    {
        // 1. ANTI-SPAM KONTROLÜ
        if (_soundTimers.TryGetValue(clip, out float lastPlayedTime))
        {
            if (Time.unscaledTime - lastPlayedTime < 0.08f) return;
        }

        _soundTimers[clip] = Time.unscaledTime;

        // 2. HAVUZDAN ÇEKME (Artık obje yaratıp silmiyoruz)
        AudioSource tempSource = Instance.GetAvailableSource();

        if (Instance.sfxSource != null)
        {
            tempSource.volume = Instance.sfxSource.volume * volumeMultiplier;
        }

        tempSource.clip = clip;
        tempSource.pitch = Random.Range(0.9f, 1.05f);
        tempSource.Play();
    }
}