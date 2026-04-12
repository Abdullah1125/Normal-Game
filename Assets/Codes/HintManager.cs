using UnityEngine;
using TMPro;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;
    public GameObject hintPanel;    // UI Paneli (Arka plan)
    public TextMeshProUGUI hintText; // UI Yazýsý

    void Awake() => Instance = this;

    public void UpdateLevelHint()
    {
        if (LevelManager.Instance?.activeLevel == null) return;
        if (LocalizationManager.Instance?.currentData == null) return;

        string currentKey = LevelManager.Instance.activeLevel.levelHint;

        if (string.IsNullOrEmpty(currentKey))
        {
            hintPanel.SetActive(false);
            return;
        }

        var field = typeof(LanguageData).GetField(currentKey);
        if (field == null)
        {
            Debug.LogWarning($"Hint key '{currentKey}' bulunamadý!");
            hintPanel.SetActive(false);
            return;
        }

        string translatedText = (string)field.GetValue(LocalizationManager.Instance.currentData);

        if (!string.IsNullOrEmpty(translatedText))
        {
            hintText.text = translatedText;
            hintPanel.SetActive(true);
        }
        else
        {
            hintPanel.SetActive(false);
        }
    }
}