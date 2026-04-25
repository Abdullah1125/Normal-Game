using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages data reset with independent panel animations and strict spam protection.
/// (Baðýmsýz panel animasyonlarý ve sýký spam korumasýyla veri sýfýrlamayý yönetir.)
/// </summary>
public class ResetManager : MonoBehaviour
{
    [Header("UI Panels (UI Panelleri)")]
    public GameObject resetConfirmationPanel;

    [Header("Panel Canvas Groups (Panel Kilit Gruplarý)")]
    public CanvasGroup resetCanvasGroup;
    public CanvasGroup settingsCanvasGroup;

    [Header("Animators (Animatörler)")]
    public MenuBounceAnimator resetAnimator;
    public MenuBounceAnimator settingsAnimator;

    private bool isResetInProgress = false;

    /// <summary>
    /// Opens the reset panel if no reset process is active.
    /// (Aktif bir sýfýrlama süreci yoksa reset panelini açar.)
    /// </summary>
    public void OpenResetPanel()
    {
        // Sýfýrlama iþlemi sürüyorsa yeni panel açma isteðini reddet
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
    /// (Sadece reset onay panelini sýký spam korumasýyla kapatýr.)
    /// </summary>
    public void CloseResetPanel()
    {
        // Eðer ana sýfýrlama iþlemi baþladýysa zaten hiçbir þey yapma
        if (isResetInProgress) return;

        // 1. KÝLÝT (HAYIR SPAM KORUMASI): 
        // Eðer panel zaten kapanma emri aldýysa (týklamalar kapalýysa) fonksiyonu durdur.
        if (resetCanvasGroup != null)
        {
            // Raycast zaten kapalýysa demek ki kapanýþ baþlamýþ, defol git diyoruz.
            if (!resetCanvasGroup.blocksRaycasts) return;

            // Deðilse hemen týklamalarý dondur (Ghost click engelleme)
            resetCanvasGroup.blocksRaycasts = false;
            resetCanvasGroup.interactable = false;
        }

        // 2. Kapanýþ animasyonunu tetikle
        if (resetAnimator != null) resetAnimator.CloseMenu();
        else if (resetConfirmationPanel != null) resetConfirmationPanel.SetActive(false);
    }

    /// <summary>
    /// Executes reset, blocks all interaction and closes panels sequentially.
    /// (Sýfýrlamayý yürütür, tüm etkileþimi kilitler ve panelleri sýrayla kapatýr.)
    /// </summary>
    public void ConfirmReset()
    {
        if (isResetInProgress) return;
        isResetInProgress = true;

        // 1. KÝLÝT: Her iki panelin de butonlarýný anýnda dondur
        if (resetCanvasGroup != null) resetCanvasGroup.blocksRaycasts = false;
        if (settingsCanvasGroup != null) settingsCanvasGroup.blocksRaycasts = false;

        DataResetProcess();

        // 2. SEKANS: Önce reset paneli kapansýn
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

        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        string lang = PlayerPrefs.GetString("SelectedLang", "English");

        PlayerPrefs.DeleteAll();

        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.SetString("SelectedLang", lang);
        PlayerPrefs.Save();

        Debug.Log("Reset: Temizleme iþlemi tamamlandý.");
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}