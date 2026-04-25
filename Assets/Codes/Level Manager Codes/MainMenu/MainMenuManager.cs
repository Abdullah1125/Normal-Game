using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages main menu interactions with anti-spam transition protection.
/// (Ana menü etkileþimlerini spam önleyici geçiþ korumasýyla yönetir.)
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels (UI Panelleri)")]
    public GameObject settingsPanel;
    public GameObject scorePanel;
    public GameObject resetPanel;

    [Header("Panel Canvas Groups (Panel Kilit Gruplarý)")]
    public CanvasGroup settingsCanvasGroup;
    public CanvasGroup scoreCanvasGroup;

    [Header("Animators (Animatörler)")]
    public MenuBounceAnimator settingsAnimator;
    public MenuBounceAnimator scoreAnimator;

    // Geçiþ devam ederken yeni komut almasýný engelleyen kilit
    private bool isTransitioning = false;

    /// <summary>
    /// Starts the game level.
    /// (Oyun seviyesini baþlatýr.)
    /// </summary>
    public void PlayGame()
    {
        if (isTransitioning || UIManager.Instance.IsMainMenuBlocked()) return;

        isTransitioning = true;
        UIManager.Instance.SetMainMenuBlock(true);

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.FadeOut(() => SceneManager.LoadScene("Levels"));
        else
            SceneManager.LoadScene("Levels");
    }

    /// <summary>
    /// Opens the settings panel safely.
    /// (Ayarlar panelini güvenli bir þekilde açar.)
    /// </summary>
    public void OpenSettings()
    {
        if (isTransitioning || UIManager.Instance.IsMainMenuBlocked()) return;

        StartCoroutine(PanelTransitionRoutine(settingsPanel, settingsCanvasGroup, true));
    }

    public void CloseSettings()
    {
        if (isTransitioning || (resetPanel != null && resetPanel.activeSelf)) return;

        StartCoroutine(PanelTransitionRoutine(settingsPanel, settingsCanvasGroup, false, settingsAnimator));
    }

    /// <summary>
    /// Opens the score panel safely.
    /// (Skor panelini güvenli bir þekilde açar.)
    /// </summary>
    public void OpenScore()
    {
        if (isTransitioning || UIManager.Instance.IsMainMenuBlocked()) return;

        StartCoroutine(PanelTransitionRoutine(scorePanel, scoreCanvasGroup, true));
    }

    public void CloseScore()
    {
        if (isTransitioning) return;

        StartCoroutine(PanelTransitionRoutine(scorePanel, scoreCanvasGroup, false, scoreAnimator));
    }

    /// <summary>
    /// Handles panel transitions and manages global UI blocks.
    /// (Panel geçiþlerini iþler ve global UI kilitlerini yönetir.)
    /// </summary>
    private IEnumerator PanelTransitionRoutine(GameObject panel, CanvasGroup group, bool opening, MenuBounceAnimator animator = null)
    {
        isTransitioning = true;

        if (opening)
        {
            // 1. Ana menüyü dondur
            UIManager.Instance.SetMainMenuBlock(true);

            // 2. Paneli ve butonlarýný hazýrla
            if (group != null) { group.blocksRaycasts = true; group.interactable = true; }
            if (resetPanel != null) resetPanel.SetActive(false);
            panel.SetActive(true);

            // Animasyon süresi kadar bekle (0.45s civarý)
            yield return new WaitForSecondsRealtime(0.5f);
        }
        else
        {
            // 1. Kapanan panelin butonlarýný anýnda dondur (Ghost Click korumasý)
            if (group != null) { group.blocksRaycasts = false; group.interactable = false; }

            // 2. Animasyonu oynat
            if (animator != null) animator.CloseMenu();
            else panel.SetActive(false);

            // Animasyonun bitmesini bekle
            yield return new WaitForSecondsRealtime(0.4f);

            // 3. Ana menüyü tekrar uyandýr
            UIManager.Instance.SetMainMenuBlock(false);
        }

        isTransitioning = false;
    }

    public void OpenResetPanel()
    {
        if (resetPanel != null) resetPanel.SetActive(true);
    }
    public void QuitGame() => Application.Quit();
    public void SetTurkish() => LocalizationManager.Instance.LoadLanguage("Turkish");
    public void SetEnglish() => LocalizationManager.Instance.LoadLanguage("English");
}