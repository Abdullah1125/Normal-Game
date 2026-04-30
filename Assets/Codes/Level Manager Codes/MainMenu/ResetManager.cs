using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages data reset with independent panel animations and strict spam protection.
/// (Bağımsız panel animasyonları ve sıkı spam korumasıyla veri sıfırlamayı yönetir.)
/// </summary>
public class ResetManager : MonoBehaviour
{
    [Header("UI Panels (UI Panelleri)")]
    public GameObject resetConfirmationPanel;

    [Header("Panel Canvas Groups (Panel Kilit Grupları)")]
    public CanvasGroup resetCanvasGroup;
    public CanvasGroup settingsCanvasGroup;

    [Header("Animators (Animatörler)")]
    public MenuBounceAnimator resetAnimator;
    public MenuBounceAnimator settingsAnimator;

    private bool isResetInProgress = false;

    /// <summary>
    /// Opens the reset panel if no reset process is active.
    /// (Aktif bir sıfırlama süreci yoksa reset panelini açar.)
    /// </summary>
    public void OpenResetPanel()
    {
        // Sıfırlama işlemi sürüyorsa yeni panel açma isteğini reddet
        if (isResetInProgress) return;

        if (resetCanvasGroup != null)
        {
            resetCanvasGroup.interactable = true;
            resetCanvasGroup.blocksRaycasts = true;
        }

        if (resetConfirmationPanel != null) resetConfirmationPanel.SetActive(true);
    }
    /// <summary>
    /// Closes only the reset confirmation panel with strict anti-spam.
    /// (Sadece reset onay panelini sıkı spam korumasıyla kapatır.)
    /// </summary>
    public void CloseResetPanel()
    {
        // Eğer ana sıfırlama işlemi başladıysa zaten hiçbir şey yapma
        if (isResetInProgress) return;

        // 1. KİLİT (HAYIR SPAM KORUMASI): 
        // Eğer panel zaten kapanma emri aldıysa (tıklamalar kapalıysa) fonksiyonu durdur.
        if (resetCanvasGroup != null)
        {
            // Raycast zaten kapalıysa demek ki kapanış başlamış, defol git diyoruz.
            if (!resetCanvasGroup.blocksRaycasts) return;

            // Değilse hemen tıklamaları dondur (Ghost click engelleme)
            resetCanvasGroup.blocksRaycasts = false;
            resetCanvasGroup.interactable = false;
        }

        // 2. Kapanış animasyonunu tetikle
        if (resetAnimator != null) resetAnimator.CloseMenu();
        else if (resetConfirmationPanel != null) resetConfirmationPanel.SetActive(false);
    }

    /// <summary>
    /// Executes reset, blocks all interaction and closes panels sequentially.
    /// (Sıfırlamayı yürütür, tüm etkileşimi kilitler ve panelleri sırayla kapatır.)
    /// </summary>
    public void ConfirmReset()
    {
        if (isResetInProgress) return;
        isResetInProgress = true;

        // 1. KİLİT: Her iki panelin de butonlarını anında dondur
        if (resetCanvasGroup != null) resetCanvasGroup.blocksRaycasts = false;
        if (settingsCanvasGroup != null) settingsCanvasGroup.blocksRaycasts = false;

        DataResetProcess();

        // 2. SEKANS: Önce reset paneli kapansın
        if (resetAnimator != null) resetAnimator.CloseMenu();

        // 3. SEKANS: Ayarlar paneli arkadan gelsin
        Invoke(nameof(CloseSettingsSequential), 0.3f);

        // 4. SEKANS: Sahne tazele
        Invoke(nameof(ReloadCurrentScene), 0.7f);
    }

    private void CloseSettingsSequential()
    {
        if (settingsAnimator != null) settingsAnimator.CloseMenu();
    }

    private void DataResetProcess()
    {
        Time.timeScale = 1f;

        float music = PlayerPrefs.GetFloat(Constants.PREF_MUSIC_VOLUME, 0.75f);
        float sfx = PlayerPrefs.GetFloat(Constants.PREF_SFX_VOLUME, 0.75f);
        string lang = PlayerPrefs.GetString(Constants.PREF_SELECTED_LANG, "English");

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat(Constants.PREF_MUSIC_VOLUME, music);
        PlayerPrefs.SetFloat(Constants.PREF_SFX_VOLUME, sfx);
        PlayerPrefs.SetString(Constants.PREF_SELECTED_LANG, lang);
        PlayerPrefs.Save();

        Debug.Log("Reset: Temizleme işlemi tamamlandı.");
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
