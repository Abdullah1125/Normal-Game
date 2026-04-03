using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    public LanguageData currentData;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // Daha önce seçilen dili yükle, yoksa Türkçe baþla
        LoadLanguage(PlayerPrefs.GetString("SelectedLang", "Turkish"));
    }

    public void LoadLanguage(string langName)
    {
        // Assets/Resources/Languages/ klasöründen JSON oku
        TextAsset jsonFile = Resources.Load<TextAsset>("Languages/" + langName);

        if (jsonFile != null)
        {
            currentData = JsonUtility.FromJson<LanguageData>(jsonFile.text);
            PlayerPrefs.SetString("SelectedLang", langName);

            // Sahnedeki LocalizedText olan her þeyi güncelle
            LocalizedText[] allTexts = FindObjectsByType<LocalizedText>(FindObjectsSortMode.None);
            foreach (var t in allTexts) t.UpdateText();

            Debug.Log(langName + " dili yüklendi.");
        }
    }
}