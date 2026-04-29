using UnityEngine;
using TMPro;

/// <summary>
/// UI metinlerini seçili dile göre otomatik olarak günceller.
/// </summary>
public class LocalizedText : MonoBehaviour
{
    [Header("Localization Settings (Yerelleþtirme Ayarlarý)")]
    [Tooltip("keyword in JSON (JSON dosyasýndaki anahtar kelime)")]
    public string key;

    private TextMeshProUGUI targetText;

    /// <summary>
    /// Bileþen referanslarýný uyanma anýnda önbelleðe alýr.
    /// </summary>
    private void Awake()
    {
        // Arama iþlemi sadece 1 kere yapýlýr ve cebe atýlýr
        targetText = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Obje sahnede aktif hale geldiðinde çeviriyi uygular.
    /// </summary>
    private void OnEnable()
    {
        UpdateText();
    }

    /// <summary>
    /// LocalizationManager üzerinden güncel veriyi çeker ve metni deðiþtirir.
    /// </summary>
    public void UpdateText()
    {
        if (LocalizationManager.Instance == null || LocalizationManager.Instance.currentData == null) return;
        if (targetText == null) return;

        // Reflection kullanarak JSON verisinden ilgili anahtarý buluyoruz
        string translatedValue = (string)typeof(LanguageData).GetField(key)?.GetValue(LocalizationManager.Instance.currentData);

        if (!string.IsNullOrEmpty(translatedValue))
        {
            // Önbellekteki referans üzerinden ýþýk hýzýnda atama yapýlýr
            targetText.text = translatedValue;
        }
    }
}