using UnityEngine;

public class SilentModePuzzle : MonoBehaviour
{
    private bool isSolved = false;

    void Update()
    {
        // 1. Senin AudioSettings'te kullandığın anahtar isimleriyle veriyi çekiyoruz
        // Eğer kayıt yoksa varsayılan 0.75f (Senin Start'taki değerin)
        float currentMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float currentSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // 2. Senin sliderların minimum değeri 0.0001f civarı olacağı için 
        // 0.01'den küçükse "sessiz" kabul ediyoruz.
        if (currentMusic <= 0.01f && currentSFX <= 0.01f)
        {
            if (!isSolved)
            {
                OpenTheGate();
                isSolved = true;
            }
        }
        else
        {
            
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
            Debug.Log("🔇 Sessizlik algılandı: Kapı Açıldı!");
        }
    }

    private void CloseTheGate()
    {
        if (GateController.Instance != null)
        {
            GateController.Instance.CloseGate();
            Debug.Log("🔊 Ses açıldı: Kapı Kapatıldı!");
        }
    }
}