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
        // Kayýtlý verileri yükle
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        ApplyVolume("MusicVol", savedMusic);
        ApplyVolume("SFXVol", savedSFX);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
    }

    void Update()
    {
        // MERMÝ BURADA: Fiziksel ses tuþlarýný dinliyoruz.
        // Eðer cihazýn sesi fiziksel olarak 0'a çekildiyse 
        // SilentModePuzzle bunu AudioListener.volume üzerinden zaten yakalayacak.

        // Eðer istersen fiziksel ses seviyesini Slider'lara da yansýtabilirsin:
        // float currentPhysicalVol = AudioListener.volume;
    }

    public void SetMusicVolume(float value)
    {
        ApplyVolume("MusicVol", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save(); // Veriyi anýnda diske yaz ki Puzzle kodu okuyabilsin
        if (value > 0.01f) isMusicMuted = false;
    }

    public void SetSFXVolume(float value)
    {
        ApplyVolume("SFXVol", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save(); // Veriyi anýnda diske yaz ki Puzzle kodu okuyabilsin
        if (value > 0.01f) isSFXMuted = false;
    }

    // Mixer logaritmik çalýþtýðý için yardýmcý fonksiyon
    private void ApplyVolume(string parameterName, float value)
    {
        float dB = Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20;
        mainMixer.SetFloat(parameterName, dB);
    }

    // Toggle fonksiyonlarýn zaten mermi gibi çalýþýyor, onlarý ellemiyorum.
    public void ToggleMusic() { /* Mevcut kodun aynýsý */ }
    public void ToggleSFX() { /* Mevcut kodun aynýsý */ }
}