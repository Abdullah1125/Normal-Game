using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [Tooltip("keyword in JSON(JSON'daki anahtar kelime)")]
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