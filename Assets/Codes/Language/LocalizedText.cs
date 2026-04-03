using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [Tooltip("JSON'daki anahtar kelime (Örn: play_button)")]
    public string key;

    void OnEnable()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (LocalizationManager.Instance == null || LocalizationManager.Instance.currentData == null) return;

        // Reflection kullanarak JSON verisinden ilgili anahtarý buluyoruz
        string translatedValue = (string)typeof(LanguageData).GetField(key)?.GetValue(LocalizationManager.Instance.currentData);

        if (!string.IsNullOrEmpty(translatedValue))
            GetComponent<TextMeshProUGUI>().text = translatedValue;
    }
}