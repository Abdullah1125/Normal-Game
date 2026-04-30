using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Professional Pause Manager with full anti-spam protection for all menu buttons.
/// (Tüm menü butonları için tam spam korumalı profesyonel Duraklatma Yöneticisi.)
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("Panels and Buttons (Paneller ve Butonlar)")]
    public GameObject pauseMenuUI;
    public GameObject settingsPanelUI;
    public GameObject hudPauseButton;
    public GameObject extraHintButton;

    public static bool isPaused = false;
    public static bool isAdLoading = false;
    private bool isToggling = false;

    [Header("Animation Controllers (Animasyon Kontrolcüleri)")]
    public MenuBounceAnimator pauseAnimator;
    public MenuBounceAnimator settingsAnimator;

    [Header("Cooldown Settings (Spam Kilidi)")]
    public float toggleCooldown = 0.45f;
    private float lastToggleTime = 0f;

    /// <summary>
    /// Resets all locks and states on start.
    /// (Başlangıçta tüm kilitleri ve durumları sıfırlar.)
    /// </summary>
    private void Start()
    {
        isToggling = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDBlock(false);
            UIManager.Instance.SetPauseBlock(false);
        }
    }

    /// <summary>
    /// Opens the settings sub-menu from the pause menu.
    /// (Ayarlar alt menüsünü pause menüsünden açar.)
    /// </summary>
    public void OpenSettings()
    {
        if (isToggling || (Time.unscaledTime - lastToggleTime < toggleCooldown)) return;
        isToggling = true;
        lastToggleTime = Time.unscaledTime;

        Debug.Log("Sistem: Ayarlar açılıyor...");

        // Menü butonlarını dondur
        if (UIManager.Instance != null) UIManager.Instance.SetPauseBlock(true);

        if (pauseAnimator != null) pauseAnimator.CloseMenu();
        else if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        if (settingsPanelUI != null) settingsPanelUI.SetActive(true);

        StartCoroutine(UnlockToggling(0.4f));
    }

    /// <summary>
    /// Closes settings and returns to the pause menu.
    /// (Ayarları kapatır ve pause menüsüne döner.)
    /// </summary>
    public void CloseSettings()
    {
        if (isToggling || (Time.unscaledTime - lastToggleTime < toggleCooldown)) return;
        isToggling = true;
        lastToggleTime = Time.unscaledTime;

        Debug.Log("Sistem: Ayarlar kapatılıyor...");

        if (settingsAnimator != null) settingsAnimator.CloseMenu();
        else if (settingsPanelUI != null) settingsPanelUI.SetActive(false);

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        // Menü butonlarını tekrar dondur (Açılış animasyonu sırasında)
        if (UIManager.Instance != null) UIManager.Instance.SetPauseBlock(true);

        StartCoroutine(UnlockToggling(0.4f));
    }

    /// <summary>
    /// Transitions to Level Selection scene.
    /// (Bölüm seçme sahnesine geçiş yapar.)
    /// </summary>
    public void GoToLevels()
    {
        if (isToggling) return;
        isToggling = true;

        Debug.Log("Sistem: Bölüm Menüsü tuşu tetiklendi!");

        if (UIManager.Instance != null) UIManager.Instance.SetPauseBlock(true);

        Time.timeScale = 1f;
        isPaused = false;

        if (pauseAnimator != null)
        {
            pauseAnimator.CloseMenu();
            Invoke(nameof(LoadLevelScene), 0.35f);
        }
        else LoadLevelScene();
    }

    private void LoadLevelScene()
    {
        if (LevelTransition.Instance != null)
            LevelTransition.Instance.FadeOut(() => SceneManager.LoadScene(Constants.SCENE_LEVELS));
        else
            SceneManager.LoadScene(Constants.SCENE_LEVELS);
    }

    public void TogglePause()
    {
       
        if (UIManager.Instance != null && UIManager.Instance.IsHUDBlocked()) return;

        if (isAdLoading || isToggling) return;
        if (isPaused) Resume(); else Pause();
    }
    public void Pause()
    {
        if (isToggling) return;
        isToggling = true;

        if (UIManager.Instance != null) UIManager.Instance.SetHUDBlock(true);

        lastToggleTime = Time.unscaledTime;
        HideWithFold(extraHintButton);
        HideWithFold(hudPauseButton);

        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        StartCoroutine(UnlockToggling(0.46f));
    }

    public void Resume()
    {
        if (isToggling) return;
        isToggling = true;

        if (UIManager.Instance != null) UIManager.Instance.SetPauseBlock(true);

        if (pauseAnimator != null) pauseAnimator.CloseMenu();
        else if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        if (settingsPanelUI != null) settingsPanelUI.SetActive(false);

        ShowWithFold(extraHintButton);
        ShowWithFold(hudPauseButton);

        Time.timeScale = 1f;
        isPaused = false;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;
        StartCoroutine(EnableUIAfterResume(0.46f));
    }

    // --- YARDIMCI MOTORLAR ---

    private IEnumerator EnableUIAfterResume(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDBlock(false);
            UIManager.Instance.SetPauseBlock(false);
        }
        isToggling = false;
    }

    private IEnumerator UnlockToggling(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (UIManager.Instance != null) UIManager.Instance.SetPauseBlock(false);
        isToggling = false;
    }

    private void HideWithFold(GameObject obj)
    {
        if (obj == null) return;
        var anim = obj.GetComponent<ButtonFoldAnimator>();
        if (anim != null && obj.activeInHierarchy) anim.HideButton(); else obj.SetActive(false);
    }

    private void ShowWithFold(GameObject obj)
    {
        if (obj == null) return;
        var anim = obj.GetComponent<ButtonFoldAnimator>();
        if (anim != null) anim.ShowButton(); else obj.SetActive(true);
    }
}
