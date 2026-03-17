using UnityEngine;
using TMPro;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;
    public GameObject hintPanel;    // UI Paneli (Arka plan)
    public TextMeshProUGUI hintText; // UI Yazýsý

    void Awake() => Instance = this;

    // Seviye her yüklendiðinde veya oyuncu her öldüðünde çaðrýlýr
    public void UpdateLevelHint()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            string text = LevelManager.Instance.activeLevel.levelHint;

            if (!string.IsNullOrEmpty(text))
            {
                hintText.text = text;
                hintPanel.SetActive(true);
            }
            else
            {
                hintPanel.SetActive(false);
            }
        }
    }
}