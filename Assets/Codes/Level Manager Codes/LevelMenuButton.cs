using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelMenuButton : MonoBehaviour
{
    public Image buttonImage;
    public TextMeshProUGUI levelText;
    private int globalIndex;
    private bool comingSoonMode;
    public void Setup(int index, LevelData data, bool isComingSoon)
    {
        globalIndex = index;
        levelText.text = (index + 1).ToString();
        Button btn = GetComponent<Button>();
        comingSoonMode = isComingSoon;

        if (data.isCompleted) buttonImage.color = Color.green;
        else if (data.isUnlocked) buttonImage.color = Color.white;
        else buttonImage.color = Color.gray;

        btn.interactable = data.isUnlocked; // Sadece açýk olanlara basýlabilir
    }

    public void OnButtonClick()
    {
        if (comingSoonMode) return;

        // Hangi map sahnesine gidecek? (0-5 -> 1Map, 6-11 -> 2Map)
        int mapNum = (globalIndex / 6) + 1;
        // Map içindeki index ne? (0,1,2,3,4,5)
        int internalIndex = globalIndex % 6;

        PlayerPrefs.SetInt("SelectedInternalIndex", internalIndex);
        SceneManager.LoadScene(mapNum + "Map");
    }
}