using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelMenuButton : MonoBehaviour
{
    public Image buttonImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelNameText;

    private int globalIndex;
    private bool comingSoonMode;

    public void Setup(int index, LevelData data, bool isComingSoon, string locName)
    {
        globalIndex = index;
        levelText.text = (index + 1).ToString();
        Button btn = GetComponent<Button>();
        comingSoonMode = isComingSoon;

        if (data.isCompleted) buttonImage.color = Color.green;
        else if (data.isUnlocked) buttonImage.color = Color.white;
        else buttonImage.color = Color.gray;

        btn.interactable = data.isUnlocked;

        if (levelNameText != null)
        {
            levelNameText.text = locName;
            levelNameText.gameObject.SetActive(data.isCompleted);
        }
    }

    public void OnButtonClick()
    {
        if (comingSoonMode) return;

        //  OPTİMİZASYON: Tüm sahneyi 2 kere taramak yerine 1 kere tarayıp hafızaya alıyoruz!
        LevelUIManager uiManager = FindFirstObjectByType<LevelUIManager>();

        int currentChapter = (globalIndex / 6);
        int chapterLastLevelIndex = (currentChapter * 6) + 5;
        bool isChapterFinished = PlayerPrefs.GetInt("LevelComplete_" + chapterLastLevelIndex, 0) == 1;

        /*if (!isChapterFinished)
          {
              int highestUnlockedInThisChapter = 0;
              for (int i = (currentChapter * 6); i <= chapterLastLevelIndex; i++)
              {
                  if (PlayerPrefs.GetInt("LevelUnlocked_" + i, 0) == 1) highestUnlockedInThisChapter = i;
              }

              if (globalIndex < highestUnlockedInThisChapter)
              {
                  if (uiManager != null)
                  {
                      string mapName = "Map";
                      if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
                      {
                          string[] titles = LocalizationManager.Instance.currentData.page_titles;
                          if (titles != null && currentChapter < titles.Length)
                          {
                              string lowerCaseTitle = titles[currentChapter].ToLower();
                              string titleCaseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerCaseTitle);
                              mapName = (currentChapter + 1) + ". " + titleCaseName;
                          }
                      }
                      uiManager.ShowWarningPanel(mapName);
                  }
                  return;
              }
          }
  */
        //  REKLAM VE SAHNE GEÇİŞ KONTROLLERİ 
        if (globalIndex > 0 && globalIndex % 6 == 0)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (uiManager != null) uiManager.StartFakeLoading(8.0f, () => { LoadMapScene(); });
                return;
            }

            if (AdMobInterstitialManager.Instance != null)
            {
                bool isAdShowing = AdMobInterstitialManager.Instance.ShowInterstitialAd(() => { LoadMapScene(); });

                if (!isAdShowing)
                {
                    if (uiManager != null) uiManager.StartFakeLoading(5.0f, () => { LoadMapScene(); });
                    else LoadMapScene();
                }
                return;
            }
        }

        ExtraHintUI.lastUnlockedLevelID = -1;
        LoadMapScene();
    }

    private void LoadMapScene()
    {
        int mapNum = (globalIndex / 6) + 1;
        PlayerPrefs.SetInt("SelectedInternalIndex", globalIndex % 6);

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene(mapNum + "Map"); });
        else
            SceneManager.LoadScene(mapNum + "Map");
    }
}