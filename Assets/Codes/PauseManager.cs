using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Panels and Buttons (Paneller ve Butonlar)")]
    public GameObject pauseMenuUI;
    public GameObject settingsPanelUI;
    public GameObject hudPauseButton;
    public GameObject extraHintButton;
    public static bool isPaused = false;
    public static bool isAdLoading = false;

    [Header("Animation Controller (Animasyon Kontrolcüsü)")]
    public MenuBounceAnimator pauseAnimator;
    public MenuBounceAnimator settingsAnimator;

    [Header("Cooldown Settings (Spam Kilidi)")]
    public float toggleCooldown = 0.4f; // Animasyon bitene kadar týklamayý yasaklar
    private float lastToggleTime = 0f;

    void Update()
    {
        if (isAdLoading) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ESC tuþuna spam atýlmasýný da engeller
            if (Time.unscaledTime - lastToggleTime < toggleCooldown) return;

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
        if (isAdLoading) return;

        // TogglePause içindeki kontroller Pause() ve Resume() içinde yapýlýyor
        if (isPaused) Resume();
        else Pause();
    }

    public void Resume()
    {
        // Sayacý güncelle
        lastToggleTime = Time.unscaledTime;

        if (pauseAnimator != null) pauseAnimator.CloseMenu();
        else pauseMenuUI.SetActive(false);

        settingsPanelUI.SetActive(false);

        // 3. Geri gelirken de jilet gibi esneyerek açýlsýnlar
        ShowWithFold(extraHintButton);
        ShowWithFold(hudPauseButton);

        // 4. ÞALTERÝ KALDIR: Butonlar ekrana geri geldiði an týklanma hakkýný geri ver!
        if (hudPauseButton != null)
            hudPauseButton.GetComponent<UnityEngine.UI.Button>().interactable = true;

        if (extraHintButton != null)
            extraHintButton.GetComponent<UnityEngine.UI.Button>().interactable = true;

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
        // 1. MÜTHÝÞ AMELÝYAT: Animasyon oynamadan hemen önce butonlarýn fiþini çek!
        // Artýk animasyon oynarken aradan sýzýp ikinci kez týklamak fiziksel olarak imkansýz.
        if (hudPauseButton != null)
            hudPauseButton.GetComponent<UnityEngine.UI.Button>().interactable = false;

        if (extraHintButton != null)
            extraHintButton.GetComponent<UnityEngine.UI.Button>().interactable = false;

        // Sayacý güncelle
        lastToggleTime = Time.unscaledTime;

        // 2. Animasyonlarý geri getirdik! Þekilli kapanacaklar.
        HideWithFold(extraHintButton);
        HideWithFold(hudPauseButton);

        pauseMenuUI.SetActive(true);
        TutorialTrigger.OnPauseToggled?.Invoke(true);

        Time.timeScale = 0f;
        isPaused = true;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
    }

    public void OpenSettings()
    {
        // KÝLÝT KONTROLÜ
        if (Time.unscaledTime - lastToggleTime < toggleCooldown) return;
        lastToggleTime = Time.unscaledTime;

        if (pauseAnimator != null) pauseAnimator.CloseMenu();
        else pauseMenuUI.SetActive(false);

        settingsPanelUI.SetActive(true);
    }

    public void CloseSettings()
    {
        // KÝLÝT KONTROLÜ
        if (Time.unscaledTime - lastToggleTime < toggleCooldown) return;
        lastToggleTime = Time.unscaledTime;

        if (settingsAnimator != null) settingsAnimator.CloseMenu();
        else settingsPanelUI.SetActive(false);

        pauseMenuUI.SetActive(true);
    }

    public void GoToLevels()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseAnimator != null)
        {
            pauseAnimator.CloseMenu();
            Invoke("LoadLevelScene", 0.3f);
        }
        else
        {
            LoadLevelScene();
        }
    }

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

    private void ShowWithFold(GameObject buttonObj)
    {
        if (buttonObj == null) return;

        ButtonFoldAnimator foldAnim = buttonObj.GetComponent<ButtonFoldAnimator>();

        if (foldAnim != null)
        {
            foldAnim.ShowButton();
        }
        else
        {
            buttonObj.SetActive(true);
        }
    }
}