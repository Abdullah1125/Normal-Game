using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel; // Ayarlar paneli objesi
    public GameObject scorePanel;    // Skor paneli objesi

    [Header("Animatörler")]
    public MenuBounceAnimator settingsAnimator; // Ayarlar animasyon scripti
    public MenuBounceAnimator scoreAnimator;    // Skor animasyon scripti

    public void PlayGame()
    {
        if (LevelTransition.Instance != null)
        {
            // Geçiþ efekti bittikten sonra sahneyi yükle
            LevelTransition.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Levels");
            });
        }
        else SceneManager.LoadScene("Levels");
    }

    public void QuitGame() => Application.Quit();

    // Paneli aç (OnEnable sayesinde otomatik zýplar/büyür)
    public void OpenSettings() => settingsPanel.SetActive(true);

    public void CloseSettings()
    {
        // Animasyonu baþlat
        if (settingsAnimator != null) settingsAnimator.CloseMenu();
        else settingsPanel.SetActive(false);
    }

    public void OpenScore() => scorePanel.SetActive(true);

    public void CloseScore()
    {
        // Animasyonu baþlat
        if (scoreAnimator != null) scoreAnimator.CloseMenu();
        else scorePanel.SetActive(false);
    }

    public void SetTurkish() => LocalizationManager.Instance.LoadLanguage("Turkish");
    public void SetEnglish() => LocalizationManager.Instance.LoadLanguage("English");
}