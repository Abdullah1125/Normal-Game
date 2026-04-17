using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    public GameObject resetConfirmationPanel;


    public void OpenResetPanel()
    {
        resetConfirmationPanel.SetActive(true);
    }

   
    public void CloseResetPanel()
    {
        resetConfirmationPanel.SetActive(false);
    }


    public void ConfirmReset()
    {
        // Ayarlarý yedekle
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        string lang = PlayerPrefs.GetString("SelectedLang", "English");

        // Seviyeleri sil (0-99 arasý)
        for (int i = 0; i < 60; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
            PlayerPrefs.DeleteKey("LevelComplete_" + i);
        }

        // Ayarlarý geri yükle
        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.SetString("SelectedLang", lang);

        // Ýlk leveli aç
        
        PlayerPrefs.Save();

        Debug.Log(" Seviyeler sýfýrlandý");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}