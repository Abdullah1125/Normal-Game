using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Panels and Buttons(Paneller ve Butonlar)")]
    public GameObject pauseMenuUI;
    public GameObject settingsPanelUI;
    public GameObject hudPauseButton;
    public GameObject extraHintButton;
    public static bool isPaused = false;

    [Header("Animation Controller(Animasyon Kontrolcüsü)")]
    public MenuBounceAnimator pauseAnimator;    // Pause paneline attýðýmýz script
    public MenuBounceAnimator settingsAnimator; // Settings paneline attýðýmýz script

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                if (settingsPanelUI.activeSelf) CloseSettings();
                else Resume();
            }
            else Pause();
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
       
        if (pauseAnimator != null) pauseAnimator.CloseMenu();
        else pauseMenuUI.SetActive(false);

        settingsPanelUI.SetActive(false);
        extraHintButton.SetActive(true);

        if (hudPauseButton != null) hudPauseButton.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;

        

        StartCoroutine(WaitAndShowTutorial(0.3f));
    }

    
    private System.Collections.IEnumerator WaitAndShowTutorial(float delay)
    {
      
        yield return new WaitForSeconds(delay);

        
        TutorialTrigger.OnPauseToggled?.Invoke(false);
    }

    public void Pause()
    {
        // SetActive(true) olduðu an Bounce efekti otomatik tetiklenir
        pauseMenuUI.SetActive(true);
        HideWithFold(extraHintButton);
        HideWithFold(hudPauseButton);

        TutorialTrigger.OnPauseToggled?.Invoke(true);

        Time.timeScale = 0f;
        isPaused = true;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
    }

    public void OpenSettings()
    {
        // Pause menüsünü animasyonla kapat, ayarlarý animasyonla aç
        if (pauseAnimator != null) pauseAnimator.CloseMenu();
        else pauseMenuUI.SetActive(false);

        settingsPanelUI.SetActive(true);
    }
    
    private void HideWithFold(GameObject buttonObj)
    {
        if (buttonObj == null) return;

        ButtonFoldAnimator foldAnim = buttonObj.GetComponent<ButtonFoldAnimator>();

        if (foldAnim != null && buttonObj.activeInHierarchy)
        {
            foldAnim.HideButton();
        }
        else
        {
            buttonObj.SetActive(false);
        }
    }

    public void CloseSettings()
    {
        // Ayarlarý animasyonla kapat, Pause menüsünü animasyonla geri aç
        if (settingsAnimator != null) settingsAnimator.CloseMenu();
        else settingsPanelUI.SetActive(false);

        pauseMenuUI.SetActive(true);
    }

    public void GoToLevels()
    {
        // 1. Önce zamaný normale döndür ve deðiþkenleri sýfýrla
        Time.timeScale = 1f;
        isPaused = false;

        // 2. Eðer animatör varsa kapanma animasyonunu baþlat
        if (pauseAnimator != null)
        {
            pauseAnimator.CloseMenu();
            // Animasyonun bitmesi için kýsa bir süre bekle (Coroutin'e geçmek yerine basitçe Invoke kullanabiliriz)
            Invoke("LoadLevelScene", 0.3f); // 0.3f senin CloseDuration sürenle ayný olmalý
        }
        else
        {
            // Eðer animatör yoksa direkt geç
            LoadLevelScene();
        }
    }

    // Sahne yükleme iþlemini ayrý bir yere aldýk
    private void LoadLevelScene()
    {
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene("Levels");
            });
        }
        else
        {
            SceneManager.LoadScene("Levels");
        }
    }
}