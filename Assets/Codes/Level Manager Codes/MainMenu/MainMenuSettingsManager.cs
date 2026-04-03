using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuSettingsManager : MonoBehaviour
{
    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}