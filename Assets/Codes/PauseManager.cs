using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Paneller ve Butonlar")]
    public GameObject pauseMenuUI;        
    public GameObject settingsPanelUI;   
    public GameObject hudPauseButton;      

    public static bool isPaused = false;

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
        pauseMenuUI.SetActive(false);
        settingsPanelUI.SetActive(false);

    
        if (hudPauseButton != null) hudPauseButton.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;
    }

    
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        settingsPanelUI.SetActive(false);

       
        if (hudPauseButton != null) hudPauseButton.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsPanelUI.SetActive(true);
        
    }

    public void CloseSettings()
    {
        settingsPanelUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void GoToLevels()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Levels");
    }
}