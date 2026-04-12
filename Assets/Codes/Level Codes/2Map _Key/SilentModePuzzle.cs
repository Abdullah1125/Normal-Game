using UnityEngine;

public class SilentModePuzzle : MonoBehaviour
{
    private bool isSolved = false;

    void Update()
    {
        // 1. Fiziksel Ses (Telefonun yan tuşları)
        float systemVolume = AudioListener.volume;

        // 2. Ayarlar Menüsü (Senin slider verilerin)
        // Eğer sliderların PlayerPrefs'e "MusicVolume" ve "SFXVolume" diye kaydediliyorsa:
        float menuMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float menuSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // MANTIĞIMIZ: 
        // EĞER (Sistem sesi kapalıysa) VEYA (Ayarlardan hem müzik hem SFX kısıldıysa)
        if (systemVolume <= 0.01f || (menuMusic <= 0.01f && menuSFX <= 0.01f))
        {
            if (!isSolved)
            {
                OpenTheGate();
                isSolved = true;
            }
        }
        else
        {
            // Eğer herhangi biri tekrar açılırsa kapıyı kapat
            if (isSolved)
            {
                CloseTheGate();
                isSolved = false;
            }
        }
    }

    private void OpenTheGate()
    {
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
            Debug.Log("🔇 Tam sessizlik sağlandı: Kapı Açıldı!");
        }
    }

    private void CloseTheGate()
    {
        if (GateController.Instance != null)
        {
            GateController.Instance.CloseGate();
            Debug.Log("🔊 Ses geri geldi: Kapı Kapatıldı!");
        }
    }
}