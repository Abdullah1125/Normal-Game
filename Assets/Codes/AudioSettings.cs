using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer mainMixer;

    [Header("UI Elemanlarý")]
    public Slider musicSlider; 
    public Slider sfxSlider;   
    private bool isMusicMuted = false;
    private bool isSFXMuted = false;

  
    public void ToggleMusic()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            mainMixer.SetFloat("MusicVol", -80f); 
            musicSlider.value = 0.0001f;         
        }
        else
        {
            mainMixer.SetFloat("MusicVol", 0f); 
            musicSlider.value = 1f;              
        }
    }

   
    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;

        if (isSFXMuted)
        {
            mainMixer.SetFloat("SFXVol", -80f);
            sfxSlider.value = 0.0001f;
        }
        else
        {
            mainMixer.SetFloat("SFXVol", 0f);
            sfxSlider.value = 1f;
        }
    }

    
    public void SetMusicVolume(float value)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
        
        if (value > 0.01f) isMusicMuted = false;
    }

    public void SetSFXVolume(float value)
    {
        mainMixer.SetFloat("SFXVol", Mathf.Log10(value) * 20);
        if (value > 0.01f) isSFXMuted = false;
    }
}