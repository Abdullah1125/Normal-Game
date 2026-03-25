using System.Collections.Generic;
using UnityEngine;

public class LevelUIManager : MonoBehaviour
{
    public List<LevelData> allGameLevels;
    public GameObject buttonPrefab;
    public Transform gridParent;

    private int currentPage = 0;
    private int levelsPerPage = 12; 
    private List<LevelMenuButton> spawnedButtons = new List<LevelMenuButton>();

    void Start()
    {
        // Ýlk açýlýþta 12 tane buton oluþtur ve listeye kaydet
        PrepareButtons();
        //Sayfayý verilerle doldur
        RefreshPage();
    }

    void PrepareButtons()
    {
        // Eski ne varsa sil
        foreach (Transform t in gridParent) Destroy(t.gameObject);
        spawnedButtons.Clear();

        // Sahnede SADECE 12 tane buton oluþturuyoruz
        {
            GameObject btnObj = Instantiate(buttonPrefab, gridParent);
            LevelMenuButton script = btnObj.GetComponent<LevelMenuButton>();
            spawnedButtons.Add(script);
        }
    }

    public void RefreshPage()
    {
        int startIndex = currentPage * levelsPerPage;

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

               
                spawnedButtons[i].Setup(currentDataIndex, data);
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
        if ((currentPage + 1) * levelsPerPage < allGameLevels.Count)
        {
            currentPage++;
            RefreshPage();
        }
    }

    // SOL TUÞ (Önceki Sayfa)
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        currentPage = 0; // Baþa dön
        RefreshPage();
    }
}