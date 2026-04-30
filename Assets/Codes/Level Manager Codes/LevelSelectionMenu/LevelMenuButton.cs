using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages level selection buttons, including locks, UI updates, and ad integration.
/// (Bölüm seçim butonlarını, kilitleri, UI güncellemelerini ve reklam entegrasyonunu yönetir.)
/// </summary>
public class LevelMenuButton : MonoBehaviour
{
    [Header("UI References (UI Referansları)")]
    public Image buttonImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI levelNameText;

    private int globalIndex;
    private bool comingSoonMode;

    // Önbellek değişkenleri
    private Button btn;
    private LevelUIManager uiManager;

    /// <summary>
    /// Caches necessary component references.
    /// (Gerekli bileşen referanslarını önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    /// <summary>
    /// Finds global managers once at start.
    /// (Global yöneticileri başlangıçta bir kez bulur.)
    /// </summary>
    private void Start()
    {
        uiManager = FindFirstObjectByType<LevelUIManager>();
    }

    /// <summary>
    /// Initializes the button state based on level data.
    /// (Buton durumunu bölüm verilerine göre başlatır.)
    /// </summary>
    public void Setup(int index, LevelData data, bool isComingSoon, string locName)
    {
        globalIndex = index;
        comingSoonMode = isComingSoon;

        if (levelText != null)
            levelText.text = (index + 1).ToString();

        if (data.isCompleted) buttonImage.color = Color.green;
        else if (data.isUnlocked) buttonImage.color = Color.white;
        else buttonImage.color = Color.gray;

        if (btn != null)
            btn.interactable = data.isUnlocked;

        if (levelNameText != null)
        {
            levelNameText.text = locName;
            levelNameText.gameObject.SetActive(data.isCompleted);
        }
    }

    /// <summary>
    /// Triggered when the button is clicked. Handles locks, ads, and scene loading.
    /// (Butona tıklandığında tetiklenir. Kilitleri, reklamları ve sahne yüklemesini yönetir.)
    /// </summary>
    public void OnButtonClick()
    {
        if (comingSoonMode) return;

        int currentChapter = globalIndex / 6;
        int chapterLastLevelIndex = (currentChapter * 6) + 5;

        bool isChapterFinished = PlayerPrefs.GetInt(Constants.PREF_LEVEL_COMPLETE_PREFIX + chapterLastLevelIndex, 0) == 1;

        if (!isChapterFinished)
        {
            if (IsLevelLockedInChapter(currentChapter, chapterLastLevelIndex)) return;
        }

        // Reklam ve Sahne Geçiş Kontrolleri
        if (globalIndex > 0 && globalIndex % 6 == 0)
        {
            HandleAdsAndLoading();
            return;
        }

        ExtraHintUI.lastUnlockedLevelID = -1;
        LoadMapScene();
    }

    /// <summary>
    /// Checks if the player is trying to skip uncompleted levels within the current chapter.
    /// (Oyuncunun mevcut chapter içinde tamamlanmamış bölümleri atlayıp atlamadığını kontrol eder.)
    /// </summary>
    private bool IsLevelLockedInChapter(int currentChapter, int chapterLastLevelIndex)
    {
        int highestUnlockedInThisChapter = 0;
        for (int i = (currentChapter * 6); i <= chapterLastLevelIndex; i++)
        {
            if (PlayerPrefs.GetInt(Constants.PREF_LEVEL_UNLOCKED_PREFIX + i, 0) == 1)
                highestUnlockedInThisChapter = i;
        }

        if (globalIndex < highestUnlockedInThisChapter)
        {
            if (uiManager != null)
            {
                string mapName = GetLocalizedMapName(currentChapter);
                uiManager.ShowWarningPanel(mapName);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Formats the map name based on localization data.
    /// (Yerelleştirme verisine göre harita adını biçimlendirir.)
    /// </summary>
    private string GetLocalizedMapName(int currentChapter)
    {
        string mapName = Constants.SCENE_MAP_SUFFIX;
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            string[] titles = LocalizationManager.Instance.currentData.page_titles;
            if (titles != null && currentChapter < titles.Length)
            {
                string lowerCaseTitle = titles[currentChapter].ToLower();
                string titleCaseName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(lowerCaseTitle);
                mapName = (currentChapter + 1) + ". " + titleCaseName;
            }
        }
        return mapName;
    }

    /// <summary>
    /// Manages interstitial ads and fake loading screens.
    /// (Geçiş reklamlarını ve sahte yükleme ekranlarını yönetir.)
    /// </summary>
    private void HandleAdsAndLoading()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (uiManager != null) uiManager.StartFakeLoading(8.0f, LoadMapScene);
            else LoadMapScene();
            return;
        }

        if (AdMobInterstitialManager.Instance != null)
        {
            bool isAdShowing = AdMobInterstitialManager.Instance.ShowInterstitialAd(LoadMapScene);

            if (!isAdShowing)
            {
                if (uiManager != null) uiManager.StartFakeLoading(5.0f, LoadMapScene);
                else LoadMapScene();
            }
        }
        else
        {
            LoadMapScene();
        }
    }

    /// <summary>
    /// Transitions to the target map scene.
    /// (Hedef harita sahnesine geçiş yapar.)
    /// </summary>
    private void LoadMapScene()
    {
        int mapNum = (globalIndex / 6) + 1;
        PlayerPrefs.SetInt(Constants.PREF_SELECTED_INTERNAL_INDEX, globalIndex % 6);
        PlayerPrefs.SetInt(Constants.PREF_LAST_LEVEL_ID, globalIndex);
        PlayerPrefs.Save();

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene(mapNum + Constants.SCENE_MAP_SUFFIX); });
        else
            SceneManager.LoadScene(mapNum + Constants.SCENE_MAP_SUFFIX);
    }
}
