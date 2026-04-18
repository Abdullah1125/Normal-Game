using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    public GameObject resetConfirmationPanel; // Reset onay paneli objesi

    [Header("Animators(Animatörler)")]
    public MenuBounceAnimator resetAnimator;   // Reset animatörü (Pop-up/Küçülen)
    public MenuBounceAnimator settingsAnimator; // Ayarlar animatörü (Aþaðý kayan)

    public void OpenResetPanel() => resetConfirmationPanel.SetActive(true);

    public void CloseResetPanel()
    {
        // Hayýr'a basýnca sadece reset paneli küçülür
        if (resetAnimator != null) resetAnimator.CloseMenu();
        else resetConfirmationPanel.SetActive(false);
    }

    public void ConfirmReset()
    {
        // 1. Verileri temizle (Leveller + Skorlar)
        DataResetProcess();

        // 2. ÖNCE Reset paneli küçülerek yok olsun
        if (resetAnimator != null) resetAnimator.CloseMenu();

        // 3. Ayarlar panelini 0.2 saniye sonra aþaðý gönder
        // (Böylece Reset paneli Ayarlar ile beraber aþaðý sürüklenmez)
        Invoke("CloseSettingsWithAnimation", 0.2f);

        // 4. Sahneyi en son yenile
        Invoke("ReloadCurrentScene", 0.5f);
    }

    private void CloseSettingsWithAnimation()
    {
        if (settingsAnimator != null) settingsAnimator.CloseMenu();
    }

    private void DataResetProcess()
    {
        // Önemli ayarlarý (Ses, Dil) yedekle
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        string lang = PlayerPrefs.GetString("SelectedLang", "English");

        // --- SEVÝYE SIFIRLAMA ---
        for (int i = 0; i < 60; i++)
        {
            PlayerPrefs.DeleteKey("LevelUnlocked_" + i);
            PlayerPrefs.DeleteKey("LevelComplete_" + i);
        }

        PlayerPrefs.DeleteKey("TotalDeaths"); // Toplam ölüm sayýsýný sil
        PlayerPrefs.DeleteKey("TotalTime");   // Toplam süreyi sil

        // ScoreManager o an sahnedeyse, deðiþkenlerini de hemen sýfýrla ki eski veri kalmasýn
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.totalDeaths = 0;
            ScoreManager.Instance.totalTime = 0f;
        }

        // Ayarlarý geri yükle ve kaydet
        PlayerPrefs.SetFloat("MusicVolume", music);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.SetString("SelectedLang", lang);
        PlayerPrefs.Save();

        Debug.Log("Sistem: Tüm seviyeler ve skorlar sýfýrlandý!");
    }

    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}