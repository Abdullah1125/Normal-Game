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
    }

    public void SetSFXVolume(float value)
    {
        ApplyVolume("SFXVol", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
        isSFXMuted = value <= 0.01f;
    }

    private void ApplyVolume(string parameterName, float value)
    {
        if (mainMixer == null) return;
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dB);
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