using UnityEngine;

public class LocalizationManager : SingletonPersistent<LocalizationManager>
{
    public LanguageData currentData;

    protected override void Awake()
    {
        base.Awake();
        LoadLanguage(PlayerPrefs.GetString(Constants.PREF_SELECTED_LANG, "English"));
    }

    public void LoadLanguage(string langName)
    {
        // Assets/Resources/Languages/ klasÃ¶rÃ¼nden JSON oku
        TextAsset jsonFile = Resources.Load<TextAsset>("Languages/" + langName);

        if (jsonFile != null)
        {
            currentData = JsonUtility.FromJson<LanguageData>(jsonFile.text);
            PlayerPrefs.SetString(Constants.PREF_SELECTED_LANG, langName);

            // Sahnedeki LocalizedText olan her ÅŸeyi gÃ¼ncelle
            LocalizedText[] allTexts = FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);
            foreach (var t in allTexts) t.UpdateText();

            Debug.Log(langName + " dili yÃ¼klendi.");
        }
    }
    /// <summary>
    /// SİHİRLİ FONKSİYON: Level ID'sine göre 12'li gruplardan doğru metni çeker.
    /// dataType: "name", "hint", "extra"
    /// </summary>
    public string GetLevelText(int levelID, string dataType)
    {
        if (currentData == null) return "";

        int page = (levelID / 12) + 1; // Hangi sayfa olduğunu bulur (1, 2, 3...)
        int index = levelID % 12;      // O sayfadaki sırasını bulur (0 ile 11 arası)

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

        return ""; // Bulunamazsa boş döndür
    }
}

