using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer mainMixer;

    [Header("UI")]
    public Slider musicSlider;
    public Slider sfxSlider;
    private bool isMusicMuted = false;
    private bool isSFXMuted = false;
    private float lastMusicVolume = 0.75f;
    private float lastSFXVolume = 0.75f;

    // ---Ortak Görsel (Sprite) Sistemi ---
    [Header("UI Images (Arayüz Çerçeveleri)")]
    public Image musicIndicatorImage; // Müzik hoparlörünün yanındaki çerçeve
    public Image sfxIndicatorImage;   // Efekt hoparlörünün yanındaki çerçeve

    [Header("Shared Volume Sprites (Ortak Görseller)")]
    public Sprite mutedSprite; // Kapalı (X) görseli
    public Sprite bar1Sprite;  // 1 Çizgi görseli
    public Sprite bar2Sprite;  // 2 Çizgi görseli
    public Sprite bar3Sprite;  // 3 Çizgi (Full) görseli
    void Start()
    {
   
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

     
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(savedMusic);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(savedSFX);

       
        ApplyVolume("MusicVol", savedMusic);
        ApplyVolume("SFXVol", savedSFX);

       
        isMusicMuted = savedMusic <= 0.01f;
        isSFXMuted = savedSFX <= 0.01f;

        UpdateMusicIcon(savedMusic);
        UpdateSFXIcon(savedSFX);
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            lastMusicVolume = musicSlider.value > 0.01f ? musicSlider.value : 0.75f;
            musicSlider.value = 0.0001f;
            SetMusicVolume(0.0001f);
        }
        else
        {
            musicSlider.value = lastMusicVolume;
            SetMusicVolume(lastMusicVolume);
        }
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;

        if (isSFXMuted)
        {
            lastSFXVolume = sfxSlider.value > 0.01f ? sfxSlider.value : 0.75f;
            sfxSlider.value = 0.0001f;
            SetSFXVolume(0.0001f);
        }
        else
        {
            sfxSlider.value = lastSFXVolume;
            SetSFXVolume(lastSFXVolume);
        }
    }

    public void SetMusicVolume(float value)
    {
        ApplyVolume("MusicVol", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
        isMusicMuted = value <= 0.01f;
        UpdateMusicIcon(value);
    }

    public void SetSFXVolume(float value)
    {
        ApplyVolume("SFXVol", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
        isSFXMuted = value <= 0.01f;
        UpdateSFXIcon(value);
    }

    private void ApplyVolume(string parameterName, float value)
    {
        if (mainMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dB);
    }
    /// <summary>
    /// Swaps the indicator sprite based on music volume.
    /// (Müzik ses seviyesine göre gösterge görselini günceller.)
    /// </summary>
    private void UpdateMusicIcon(float volume)
    {
        if (musicIndicatorImage == null) return;

        if (volume <= 0.01f) musicIndicatorImage.sprite = mutedSprite;
        else if (volume <= 0.33f) musicIndicatorImage.sprite = bar1Sprite;
        else if (volume <= 0.66f) musicIndicatorImage.sprite = bar2Sprite;
        else musicIndicatorImage.sprite = bar3Sprite;
    }

    /// <summary>
    /// Swaps the indicator sprite based on SFX volume.
    /// (Efekt ses seviyesine göre gösterge görselini günceller.)
    /// </summary>
    private void UpdateSFXIcon(float volume)
    {
        if (sfxIndicatorImage == null) return;

        if (volume <= 0.01f) sfxIndicatorImage.sprite = mutedSprite;
        else if (volume <= 0.33f) sfxIndicatorImage.sprite = bar1Sprite;
        else if (volume <= 0.66f) sfxIndicatorImage.sprite = bar2Sprite;
        else sfxIndicatorImage.sprite = bar3Sprite;
    }

    public static float GetAndroidPhysicalVolume()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
                AndroidJavaObject audioManager = context.Call<AndroidJavaObject>("getSystemService", "audio");

                int currentVolume = audioManager.Call<int>("getStreamVolume", 3);
                int maxVolume = audioManager.Call<int>("getStreamMaxVolume", 3);

                return (float)currentVolume / maxVolume; 
            }
        }
        catch (System.Exception)
        {
            return AudioListener.volume; 
        }
#else
        return AudioListener.volume;
#endif
    }
}