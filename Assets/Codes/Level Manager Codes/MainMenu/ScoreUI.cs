using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("Total Data(Toplam Veriler)")]
    public TextMeshProUGUI totalDeathText;
    public TextMeshProUGUI totalTimeText;

  
    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
         
            ScoreManager.Instance.SaveOfflineData();

        
            string deathPrefix = "Total Death:";
            string timePrefix = "Total Time:";

          
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
            {
                var langData = LocalizationManager.Instance.currentData;

                if (!string.IsNullOrEmpty(langData.total_death))
                    deathPrefix = langData.total_death; 

                if (!string.IsNullOrEmpty(langData.total_time))
                    timePrefix = langData.total_time;
            }

          
            if (totalDeathText != null)
                totalDeathText.text = deathPrefix + " " + ScoreManager.Instance.totalDeaths;

            if (totalTimeText != null)
                totalTimeText.text = timePrefix + " " + ScoreManager.Instance.GetFormattedTime(ScoreManager.Instance.totalTime);
        }
    }
}