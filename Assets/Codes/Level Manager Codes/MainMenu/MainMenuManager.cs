using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages main menu interactions with anti-spam transition protection.
/// (Ana menü etkileşimlerini spam önleyici geçiş korumasıyla yönetir.)
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels (UI Panelleri)")]
    public GameObject settingsPanel;
    public GameObject scorePanel;
    public GameObject resetPanel;

    [Header("Panel Canvas Groups (Panel Kilit Grupları)")]
    public CanvasGroup settingsCanvasGroup;
    public CanvasGroup scoreCanvasGroup;

    [Header("Animators (Animatörler)")]
    public MenuBounceAnimator settingsAnimator;
    public MenuBounceAnimator scoreAnimator;

    // Geçiş devam ederken yeni komut almasını engelleyen kilit
    private bool isTransitioning = false;

    /// <summary>
    /// Starts the game level.
    /// (Oyun seviyesini başlatır.)
    /// </summary>
    public void PlayGame()
    {
        if (isTransitioning || UIManager.Instance.IsMainMenuBlocked()) return;

        isTransitioning = true;
        UIManager.Instance.SetMainMenuBlock(true);

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.FadeOut(() => SceneManager.LoadScene(Constants.SCENE_LEVELS));
        else
            SceneManager.LoadScene(Constants.SCENE_LEVELS);
    }

    /// <summary>
    /// Opens the settings panel safely.
    /// (Ayarlar panelini güvenli bir şekilde açar.)
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
    /// (Skor panelini güvenli bir şekilde açar.)
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
    /// (Panel geçişlerini işler ve global UI kilitlerini yönetir.)
    /// </summary>
    private IEnumerator PanelTransitionRoutine(GameObject panel, CanvasGroup group, bool opening, MenuBounceAnimator animator = null)
    {
        isTransitioning = true;

        if (opening)
        {
            // 1. Ana menüyü dondur
            UIManager.Instance.SetMainMenuBlock(true);

            // 2. Paneli ve butonlarını hazırla
            if (group != null) { group.blocksRaycasts = true; group.interactable = true; }
            if (resetPanel != null) resetPanel.SetActive(false);
            panel.SetActive(true);

            // Animasyon süresi kadar bekle (0.45s civarı)
            yield return new WaitForSecondsRealtime(0.5f);
        }
        else
        {
            // 1. Kapanan panelin butonlarını anında dondur (Ghost Click koruması)
            if (group != null) { group.blocksRaycasts = false; group.interactable = false; }

            // 2. Animasyonu oynat
            if (animator != null) animator.CloseMenu();
            else panel.SetActive(false);

            // Animasyonun bitmesini bekle
            yield return new WaitForSecondsRealtime(0.4f);

            // 3. Ana menüyü tekrar uyandır
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
