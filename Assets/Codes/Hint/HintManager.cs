using UnityEngine;
using TMPro;

public class HintManager : Singleton<HintManager>
{
    public GameObject hintPanel;    
    public TextMeshProUGUI hintText;

    protected override void Awake() { base.Awake(); }

    public void UpdateLevelHint()
    {
        if (LevelManager.Instance?.activeLevel == null) return;
        if (LocalizationManager.Instance?.currentData == null) return;

        int currentLevelID = LevelManager.Instance.activeLevel.levelID;

        // Jilet dokunuþu: Veriyi sihirli fonksiyondan iste
        string translatedText = LocalizationManager.Instance.GetLevelText(currentLevelID, "hint");

        if (!string.IsNullOrEmpty(translatedText))
        {
            hintText.text = translatedText;
            hintPanel.SetActive(true);
            return;
        }

        hintPanel.SetActive(false);
    }
}
