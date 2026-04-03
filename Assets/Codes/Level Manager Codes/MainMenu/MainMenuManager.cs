using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject scorePanel;

    public void PlayGame() { SceneManager.LoadScene("Levels"); } 
    public void QuitGame() { Application.Quit(); }

    // Panel Kontrolleri
    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);
    public void OpenScore() => scorePanel.SetActive(true);
    public void CloseScore() => scorePanel.SetActive(false);

    public void SetTurkish() => LocalizationManager.Instance.LoadLanguage("Turkish");
    public void SetEnglish() => LocalizationManager.Instance.LoadLanguage("English");
}