using UnityEngine;

public class SilentModePuzzle : MonoBehaviour
{
    private bool isSolved = false;
    private float checkTimer = 0f;
    private float checkInterval = 0.2f; 

    void Update()
    {
     
        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval) return;
        checkTimer = 0f;

        CheckSilenceStatus();
    }

    void CheckSilenceStatus()
    {
        float systemVolume = AudioSettings.GetAndroidPhysicalVolume();

        
        float menuMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float menuSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        
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
      
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
            Debug.Log(" Tam sessizlik sağlandı: Kapı Açıldı!");
        }
    }

    private void CloseTheGate()
    {
        if (GateController.Instance != null)
        {
            GateController.Instance.CloseGate();
            Debug.Log(" Ses geri geldi: Kapı Kapatıldı!");
        }
    }
}