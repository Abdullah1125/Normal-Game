using UnityEngine;

/// <summary>
/// Manages language data and provides localized texts based on the selected language.
/// (Dil verilerini yönetir ve seçilen dile göre yerelleştirilmiş metinleri sağlar.)
/// </summary>
public class LocalizationManager : SingletonPersistent<LocalizationManager>
{
    [Header("Localization Data (Yerelleştirme Verisi)")]
    public LanguageData currentData;

    [Header("Mechanic Settings (Mekanik Ayarları)")]
    [Tooltip("The specific level ID where the gyro/drag box mechanic is used. (Jiroskop/sürükleme kutusu mekaniğinin kullanıldığı spesifik bölüm ID'si.)")]
    public int targetGyroLevelID = 14; // Sürüklenen kutu bölümünün ID'sini buradan veya Inspector'dan ayarla

    /// <summary>
    /// Initializes the manager and loads the previously selected language.
    /// (Yöneticisi başlatır ve daha önce seçilen dili yükler.)
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        LoadLanguage(PlayerPrefs.GetString(Constants.PREF_SELECTED_LANG, "English"));
    }

    /// <summary>
    /// Loads the language JSON file from the Resources folder and updates all localized texts in the scene.
    /// (Resources klasöründen dil JSON dosyasını yükler ve sahnedeki tüm yerelleştirilmiş metinleri günceller.)
    /// </summary>
    public void LoadLanguage(string langName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Languages/" + langName);
        if (jsonFile != null)
        {
            currentData = JsonUtility.FromJson<LanguageData>(jsonFile.text);
            PlayerPrefs.SetString(Constants.PREF_SELECTED_LANG, langName);

            // Sahnedeki tüm metinleri bul ve yenile
            LocalizedText[] allTexts = FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);
            foreach (var t in allTexts) t.UpdateText();

            Debug.Log(langName + " dili yüklendi.");
        }
    }

    /// <summary>
    /// Retrieves the appropriate text string based on the level ID and requested data type.
    /// Includes a fallback mechanism for the specific gyro level if no accelerometer is present.
    /// (Bölüm ID'sine ve istenen veri türüne göre uygun metni çeker. Sensör yoksa belirli jiroskop bölümü için yedek mekanizma içerir.)
    /// </summary>
    public string GetLevelText(int levelID, string dataType)
    {
        if (currentData == null) return "";

        // Sadece belirlenen gyro bölümündeysek ve (sensör yoksa veya Editor'deysek) yedek metni yolla
        if (levelID == targetGyroLevelID && (!SystemInfo.supportsAccelerometer || Application.isEditor))
        {
            if (dataType == "name" && !string.IsNullOrEmpty(currentData.gyro_level_name)) return currentData.gyro_level_name;
            if (dataType == "hint" && !string.IsNullOrEmpty(currentData.gyro_hint)) return currentData.gyro_hint;
            if (dataType == "extra" && !string.IsNullOrEmpty(currentData.gyro_extra_hint)) return currentData.gyro_extra_hint;
        }

        // Normal oyun mantığı devam eder
        int page = (levelID / 12) + 1;
        int index = levelID % 12;

        string[] targetArray = null;

        switch (page)
        {
            case 1:
                if (dataType == "name") targetArray = currentData.level_names_1;
                else if (dataType == "hint") targetArray = currentData.hints_1;
                else if (dataType == "extra") targetArray = currentData.extra_hints_1;
                break;
            case 2:
                if (dataType == "name") targetArray = currentData.level_names_2;
                else if (dataType == "hint") targetArray = currentData.hints_2;
                else if (dataType == "extra") targetArray = currentData.extra_hints_2;
                break;
            case 3:
                if (dataType == "name") targetArray = currentData.level_names_3;
                else if (dataType == "hint") targetArray = currentData.hints_3;
                else if (dataType == "extra") targetArray = currentData.extra_hints_3;
                break;
            case 4:
                if (dataType == "name") targetArray = currentData.level_names_4;
                else if (dataType == "hint") targetArray = currentData.hints_4;
                else if (dataType == "extra") targetArray = currentData.extra_hints_4;
                break;
            case 5:
                if (dataType == "name") targetArray = currentData.level_names_5;
                else if (dataType == "hint") targetArray = currentData.hints_5;
                else if (dataType == "extra") targetArray = currentData.extra_hints_5;
                break;
        }

        if (targetArray != null && index < targetArray.Length)
        {
            return targetArray[index];
        }

        return "";
    }
}