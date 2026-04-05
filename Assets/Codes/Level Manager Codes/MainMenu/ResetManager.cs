using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    public GameObject resetConfirmationPanel;


    public void OpenResetPanel()
    {
        resetConfirmationPanel.SetActive(true);
    }

   
    public void CloseResetPanel()
    {
        resetConfirmationPanel.SetActive(false);
    }

    
    public void ConfirmReset()
    {
      
        Debug.Log("Tüm ilerleme sýfýrlandý!");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

       
    }
}