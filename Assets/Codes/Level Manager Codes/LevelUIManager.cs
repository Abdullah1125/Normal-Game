using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;     
public class LevelUIManager : MonoBehaviour
{
    public List<LevelData> allGameLevels;
    public GameObject buttonPrefab;
    public Transform gridParent;

    private int currentPage = 0;
    private int levelsPerPage = 12; 
    private List<LevelMenuButton> spawnedButtons = new List<LevelMenuButton>();
    public GameObject comingSoonPanel;

    [Header("Pagination Settings")]
    public GameObject dotPrefab;    // Tek bir noktanýn Prefab hali (UI Image)
    public Transform dotsParent;    // Noktalarýn dizileceði yer (Horizontal Layout Group olan obje)
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Page Titles")]
    public TMP_Text Level1Text;
    public TMP_Text Level2Text;

    private List<Image> spawnedDots = new List<Image>();
    public List<string> Level1;
    public List<string> Level2;

    void Start()
    {
        // Ýlk açýlýþta 12 tane buton oluþtur ve listeye kaydet
        PrepareButtons();
        //Sayfa üstündeki hangi sayfada olduðumuzu gösteren noktalar
        CreatePaginationDots();
        //Sayfayý verilerle doldur
        RefreshPage();
    }
    void PrepareButtons()
    {
        // Eski ne varsa sil
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
         
        int totalPages  = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);

        for (int i = 0; i < totalPages; i++) 
        {
            GameObject dot = Instantiate(dotPrefab, dotsParent);
            Image dotImage = dot.GetComponent<Image>();
            spawnedDots.Add(dotImage);
        }

    }

    void UpdatePaginationDots()
    {
        for (int i = 0; i < spawnedDots.Count;i++)
        {
            spawnedDots[i].color = (i == currentPage) ? activeColor : inactiveColor;
        }
    }
    void UpdateTexts()
    {
        if (Level1 != null && currentPage < Level1.Count)
        {
            Level1Text.text = Level1[currentPage];
        }
        if (Level2 != null && currentPage < Level2.Count)
        {
            Level2Text.text = Level2[currentPage];
        }
        
    }
    public void RefreshPage()
    {
        UpdateTexts();
        UpdatePaginationDots();
       
        int startIndex = currentPage * levelsPerPage;

        if (comingSoonPanel != null)
            comingSoonPanel.SetActive(currentPage > 0);

        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            int currentDataIndex = startIndex + i;

            // Eðer o indexte bir level verisi varsa butonu güncelle
            if (currentDataIndex < allGameLevels.Count)
            {
                spawnedButtons[i].gameObject.SetActive(true);
                LevelData data = allGameLevels[currentDataIndex];

                // Verileri diskten tazele (Kilit/Tamamlanma)
                data.isUnlocked = PlayerPrefs.GetInt("LevelUnlocked_" + data.levelID, data.levelID == 0 ? 1 : 0) == 1;
                data.isCompleted = PlayerPrefs.GetInt("LevelComplete_" + data.levelID, 0) == 1;

                bool isComingSoon = (currentPage > 0);
                spawnedButtons[i].Setup(currentDataIndex, data, isComingSoon);
            }
            else
            {
               
                spawnedButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // SAÐ TUÞ (Sonraki Sayfa)
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

    // SOL TUÞ (Önceki Sayfa)
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

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        currentPage = 0; // Baþa dön
        RefreshPage();
    }
}