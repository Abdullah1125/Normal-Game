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
}

