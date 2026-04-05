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
        mainMixer.SetFloat("MusicVol", Mathf.Log10(savedMusic) * 20);
        mainMixer.SetFloat("SFXVol", Mathf.Log10(savedSFX) * 20);
        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
    }

    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            lastMusicVolume = musicSlider.value;
            SetMusicVolume(0.0001f);
            musicSlider.value = 0.0001f;         
        }
        else
        {
            SetMusicVolume(lastMusicVolume);    
            musicSlider.value = lastMusicVolume;
        }
    }

   
    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;

        if (isSFXMuted)
        {
            lastSFXVolume = sfxSlider.value;   
            SetSFXVolume(0.0001f);
            sfxSlider.value = 0.0001f;
        }
        else
        {
            SetSFXVolume(lastSFXVolume);        
            sfxSlider.value = lastSFXVolume;
        }
    }


    public void SetMusicVolume(float value)
    {
       
        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (value > 0.01f) isMusicMuted = false;
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
        if (value > 0.01f) isSFXMuted = false;
    }
}