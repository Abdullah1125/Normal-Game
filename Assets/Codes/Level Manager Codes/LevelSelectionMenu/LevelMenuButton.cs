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
            if (data.isCompleted)
            {
                // UIManager'dan gelen dili yazdýr
                levelNameText.text = locName;
                levelNameText.gameObject.SetActive(true);
            }
            else
            {
                levelNameText.gameObject.SetActive(false);
            }
        }
    }

    public void OnButtonClick()
    {
        if (comingSoonMode) return;

        // Hangi map sahnesine gidecek? (0-5 -> 1Map, 6-11 -> 2Map)
        int mapNum = (globalIndex / 6) + 1;
        // Map içindeki index ne? (0,1,2,3,4,5)
        int internalIndex = globalIndex % 6;

        PlayerPrefs.SetInt("SelectedInternalIndex", internalIndex);
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.FadeOut(() =>
            {
                SceneManager.LoadScene(mapNum + "Map");
            });
        }
        else
        {
            SceneManager.LoadScene(mapNum + "Map");
        }

    }
}