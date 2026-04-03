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
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            // 1. LevelData içindeki anahtarý (hintKey) alýyoruz
            string currentKey = LevelManager.Instance.activeLevel.levelHint;

            // 2. Eðer anahtar boþ deðilse dile göre metni çekiyoruz
            if (!string.IsNullOrEmpty(currentKey))
            {
                // LocalizationManager üzerinden çeviriyi alýyoruz
                // Not: LocalizedText scriptindeki mantýðý burada direkt kullanýyoruz
                string translatedText = (string)typeof(LanguageData)
                    .GetField(currentKey)
                    ?.GetValue(LocalizationManager.Instance.currentData);

                if (!string.IsNullOrEmpty(translatedText))
                {
                    hintText.text = translatedText;
                    hintPanel.SetActive(true);
                }
                else
                {
                    // Anahtar JSON'da bulunamadýysa paneli kapat veya hata bas
                    hintPanel.SetActive(false);
                    Debug.LogWarning(currentKey + " anahtarý JSON dosyasýnda bulunamadý!");
                }
            }
            else
            {
                // Eðer seviyede ipucu yoksa (boþ býrakýldýysa) paneli gizle
                hintPanel.SetActive(false);
            }
        }
    }
}