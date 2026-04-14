using UnityEngine;

public class SilentModePuzzle : MonoBehaviour
{
    private bool isSolved = false;
    private float checkTimer = 0f;
    private float checkInterval = 0.2f; // Saniyede 5 kez kontrol et (Performans için ideal)

    void Update()
    {
        // Her kare yerine belirli aralıklarla kontrol edelim (Optimasyon)
        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return;
        checkTimer = 0f;

        CheckSilenceStatus();
    }

    void CheckSilenceStatus()
    {
        // 1. Fiziksel Ses (0.0 ile 1.0 arası)
        float systemVolume = AudioListener.volume;

        // 2. Ayarlar Menüsü (PlayerPrefs okumasını optimize ettik)
        float menuMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float menuSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        // MANTIĞIMIZ: 
        // Ya telefonun sesi tamamen kısılacak, 
        // YA DA menüden hem müzik hem de ses efektleri 0'a çekilecek.
        bool isSilent = (systemVolume <= 0.01f) || (menuMusic <= 0.01f && menuSFX <= 0.01f);

        if (isSilent)
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
        // GateController.Instance kontrolü mermi gibi olmuş, hata almanı engeller
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