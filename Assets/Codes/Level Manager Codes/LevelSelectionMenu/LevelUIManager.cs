using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    public List<LevelData> allGameLevels;
    public GameObject buttonPrefab;
    public Transform gridParent;

    private int currentPage = 0;
    private int levelsPerPage = 12;
    private List<LevelMenuButton> spawnedButtons = new List<LevelMenuButton>();
    public GameObject comingSoonPanel;

    [Header("Pagination Settings(Sayfalandýrma Ayarlarý)")]
    public GameObject dotPrefab;
    public Transform dotsParent;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Page Titles(Sayfa Baþlýklarý)")]
    public TMP_Text Level1Text;
    public TMP_Text Level2Text;

    [Header("Warning Settings(Uyarý Ayarlarý)")]
    public CanvasGroup warningPanelCG;

    private List<Image> spawnedDots = new List<Image>();

    // JSON'dan dolacak listeler
    public List<string> Level1 = new List<string>();
    public List<string> Level2 = new List<string>();

    void Start()
    {
        PrepareButtons();
        CreatePaginationDots();
        RefreshPage();
    }
    public void ShowWarningPanel()
    {
        if (warningPanelCG == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeWarningPanelRoutine());
    }

    private System.Collections.IEnumerator FadeWarningPanelRoutine()
    {
        warningPanelCG.gameObject.SetActive(true);

        // 0.4 saniyede belir
        float t = 0;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            warningPanelCG.alpha = t / 0.4f;
            yield return null;
        }

        // 1.2 saniye bekle
        yield return new WaitForSeconds(1.2f);

        // 0.4 saniyede kaybol
        while (t > 0)
        {
            t -= Time.deltaTime;
            warningPanelCG.alpha = t / 0.4f;
            yield return null;
        }

        warningPanelCG.gameObject.SetActive(false);
    }

    void PrepareButtons()
    {
        foreach (Transform t in gridParent) Destroy(t.gameObject);
        spawnedButtons.Clear();

        for (int i = 0; i < levelsPerPage; i++)
        {
            GameObject btnObj = Instantiate(buttonPrefab, gridParent);
            LevelMenuButton script = btnObj.GetComponent<LevelMenuButton>();
            spawnedButtons.Add(script);
        }
    }

    void CreatePaginationDots()
    {
        foreach (Transform t in dotsParent) Destroy(t.gameObject);
        spawnedDots.Clear();

        int totalPages = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);

        for (int i = 0; i < totalPages; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsParent);
            Image dotImage = dot.GetComponent<Image>();
            spawnedDots.Add(dotImage);
        }
    }

    void UpdatePaginationDots()
    {
        for (int i = 0; i < spawnedDots.Count; i++)
        {
            spawnedDots[i].color = (i == currentPage) ? activeColor : inactiveColor;
        }
    }

    void UpdateTexts()
    {
        if (LocalizationManager.Instance == null || LocalizationManager.Instance.currentData == null) return;

        var data = LocalizationManager.Instance.currentData;

        if (data.page_titles == null) return;

        int baseIndex = currentPage * 2;

        if (baseIndex < data.page_titles.Length)
        {
            Level1Text.text = data.page_titles[baseIndex];
        }
        else
        {
            Level1Text.text = "CHAPTER " + (currentPage + 1);
        }

        if (Level2Text != null)
        {
            if (baseIndex + 1 < data.page_titles.Length)
            {
                Level2Text.text = data.page_titles[baseIndex + 1];
            }
            else
            {
                Level2Text.text = "";
            }
        }
    }

    public void RefreshPage()
    {
        UpdateTexts();
        UpdatePaginationDots();

        int startIndex = currentPage * levelsPerPage;

        if (comingSoonPanel != null)
            comingSoonPanel.SetActive(currentPage > 0);

        // JSON listelerini C# listelerine aktar
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            if (LocalizationManager.Instance.currentData.Level1 != null)
                Level1 = new List<string>(LocalizationManager.Instance.currentData.Level1);

          
        }

        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            int currentDataIndex = startIndex + i;

            if (currentDataIndex < allGameLevels.Count)
            {
                spawnedButtons[i].gameObject.SetActive(true);
                LevelData data = allGameLevels[currentDataIndex];

                data.isUnlocked = PlayerPrefs.GetInt("LevelUnlocked_" + data.levelID, data.levelID == 0 ? 1 : 0) == 1;
                data.isCompleted = PlayerPrefs.GetInt("LevelComplete_" + data.levelID, 0) == 1;

                bool isComingSoon = (currentPage > 0);

                string localizedLevelName = data.levelName;

                if (currentPage == 0 && i < Level1.Count)
                {
                    localizedLevelName = Level1[i];
                }
                else if (currentPage == 1 && i < Level2.Count)
                {
                    localizedLevelName = Level2[i];
                }

                // Butonu kur
                spawnedButtons[i].Setup(currentDataIndex, data, isComingSoon, localizedLevelName);
            }
            else
            {
                spawnedButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void NextPage()
    {
        int totalPages = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);
        if (currentPage >= totalPages - 1)
        {
            currentPage = 0;
        }
        else currentPage++;

        RefreshPage();
    }

    public void PreviousPage()
    {
        int totalPages = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);

        if (currentPage <= 0)
        {
            currentPage = totalPages - 1;
        }
        else currentPage--;
        RefreshPage();
    }

    public void MainMenu()
    {
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.SlideDownToScene("MainMenu");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}