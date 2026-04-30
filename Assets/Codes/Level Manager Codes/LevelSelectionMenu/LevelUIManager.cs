﻿﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    public List<LevelData> allGameLevels;
    public GameObject buttonPrefab;
    public Transform gridParent;
    private Vector2 originalGridPos;

    private int currentPage = 0;
    private int levelsPerPage = 12;
    private List<LevelMenuButton> spawnedButtons = new List<LevelMenuButton>();
    public GameObject comingSoonPanel;

    [Header("Page Backgrounds(Sayfa Arka PlanlarÄ±)")]
    public Image backgroundImageTop;
    public RectTransform bgRectTop;
    public Image backgroundImageBottom;
    public RectTransform bgRectBottom;
    public List<Sprite> pageBackgrounds;

    [Header("Page Transition Settings(Sayfa GeÃ§iÅŸ AyarlarÄ±)")]
    public CanvasGroup gridCanvasGroup;
    public RectTransform gridRect;
    public float slideDistance = 1920f;
    public float slideDuration = 0.15f;

    [Header("Coming Soon Settings(Ã‡ok YakÄ±nda AyarlarÄ±)")]
    public float comingSoonFadeDuration = 0.4f;
    private Coroutine comingSoonCoroutine;

    private bool isAnimating = false;

    [Header("Text Settings(YazÄ± AyarlarÄ±)")]
    public bool reverseTextDirection = false;

    private TMP_Text dummyLevel1Text;
    private TMP_Text dummyLevel2Text;
    private RectTransform text1Rect;
    private RectTransform text2Rect;
    private RectTransform dummyText1Rect;
    private RectTransform dummyText2Rect;
    private Vector2 origText1Pos;
    private Vector2 origText2Pos;

    [Header("Pagination Settings(SayfalandÄ±rma AyarlarÄ±)")]
    public GameObject dotPrefab;
    public Transform dotsParent;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Page Titles(Sayfa BaÅŸlÄ±klarÄ±)")]
    public TMP_Text Level1Text;
    public TMP_Text Level2Text;

    [Header("Warning Settings(UyarÄ± AyalarÄ±)")]
    public CanvasGroup warningPanelCG;
    public TextMeshProUGUI warningPanelText;

    [Header("Grid Layout Settings(Izgara Boyut AyarlarÄ±)")]
    private Vector2 originalCellSize;
    private Vector2 originalSpacing;
    private bool isGridInitialized = false;

    private List<Image> spawnedDots = new List<Image>();

    public List<string> Level1 = new List<string>();
    public List<string> Level2 = new List<string>();

    [Header("Object Pooling(Nesne Havuzlama)")]
    private GameObject dummyGridObj;
    private RectTransform dummyRect;
    private List<LevelMenuButton> dummyButtons = new List<LevelMenuButton>();

    public GameObject loadingAdPanel; 
    public TextMeshProUGUI loadingText;

    // Onbellege alinan bilesenler
    private CanvasGroup comingSoonCG;
    private LocalizedText warningLocText;
    private GridLayoutGroup cachedGridLayout;
    private RectTransform cachedCanvasRect;
    private CanvasScaler cachedCanvasScaler;

    private Coroutine _dotsCoroutine;
    void Start()
    {
        int lastLevelID = PlayerPrefs.GetInt(Constants.PREF_LAST_LEVEL_ID, 0);
        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage));
        currentPage = Mathf.Clamp(lastLevelID / levelsPerPage, 0, totalPages - 1);

        if (gridRect == null) gridRect = gridParent.GetComponent<RectTransform>();

        // Grid'in editÃ¶rdeki orijinal pozisyonunu hafÄ±zaya alÄ±r
        originalGridPos = gridRect.anchoredPosition;

        // Tam ekran geniÅŸliÄŸinde kaydÄ±rma yapabilmek iÃ§in Canvas'Ä±n gerÃ§ek geniÅŸliÄŸini alÄ±r
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas != null)
        {
            cachedCanvasRect = canvas.rootCanvas.GetComponent<RectTransform>();
            cachedCanvasScaler = canvas.rootCanvas.GetComponent<CanvasScaler>();
            if (cachedCanvasRect != null)
            {
                slideDistance = cachedCanvasRect.rect.width;
            }
        }
        else
        {
            slideDistance = Screen.width;
        }

        AdjustGridLayout();

        PrepareButtons();
        CreateDummyPool();
        CreatePaginationDots();

        // Arka plan gÃ¶rsellerini ve ilk sayfa pozisyonlarÄ±nÄ± ayarlar
        if (backgroundImageTop != null && pageBackgrounds != null && pageBackgrounds.Count > 0)
        {
            if (currentPage < pageBackgrounds.Count) backgroundImageTop.sprite = pageBackgrounds[currentPage];
            backgroundImageTop.gameObject.SetActive(true);
        }
        if (bgRectTop != null) bgRectTop.anchoredPosition = originalGridPos;
        if (backgroundImageBottom != null) backgroundImageBottom.gameObject.SetActive(false);

        // BaÅŸlangÄ±Ã§ sayfasÄ±nÄ±n kilit (Coming Soon) durumunu kontrol eder
        if (comingSoonPanel != null)
        {
            comingSoonCG = comingSoonPanel.GetComponent<CanvasGroup>();
            if (comingSoonCG == null) comingSoonCG = comingSoonPanel.AddComponent<CanvasGroup>();
            comingSoonCG.alpha = (currentPage > 1) ? 1f : 0f;
            comingSoonPanel.SetActive(currentPage > 1);
        }

        RefreshPageUI();
        FillGridWithPageData(currentPage, spawnedButtons);
    }

    
    // Animasyonlarda render yÃ¼kÃ¼nÃ¼ azaltmak iÃ§in yedek tablo ve yazÄ± kopyalarÄ±nÄ± oluÅŸturur
    void CreateDummyPool()
    {
        dummyGridObj = Instantiate(gridParent.gameObject, gridParent.parent);
        dummyGridObj.name = "Optimized_DummyGrid";
        dummyRect = dummyGridObj.GetComponent<RectTransform>();

        // KopyanÄ±n anchor ve pivot ayarlarÄ±nÄ± orijinaliyle eÅŸitler
        dummyRect.anchorMin = gridRect.anchorMin;
        dummyRect.anchorMax = gridRect.anchorMax;
        dummyRect.pivot = gridRect.pivot;
        dummyRect.sizeDelta = gridRect.sizeDelta;
        dummyGridObj.transform.localScale = gridRect.localScale;

        // YazÄ±larÄ±n RectTransform referanslarÄ±nÄ± ve orijinal konumlarÄ±nÄ± saklar
        text1Rect = Level1Text != null ? Level1Text.GetComponent<RectTransform>() : null;
        text2Rect = Level2Text != null ? Level2Text.GetComponent<RectTransform>() : null;
        if (text1Rect != null) origText1Pos = text1Rect.anchoredPosition;
        if (text2Rect != null) origText2Pos = text2Rect.anchoredPosition;

        // Havuz nesnesinin iÃ§ini temizleyip yeni butonlar ekler
        foreach (Transform t in dummyGridObj.transform) Destroy(t.gameObject);
        for (int i = 0; i < levelsPerPage; i++)
        {
            GameObject btnObj = Instantiate(buttonPrefab, dummyGridObj.transform);
            dummyButtons.Add(btnObj.GetComponent<LevelMenuButton>());
        }

        // Havuz nesnesinin tÄ±klanmasÄ±nÄ± engeller
        CanvasGroup dummyCG = dummyGridObj.GetComponent<CanvasGroup>();
        if (dummyCG == null) dummyCG = dummyGridObj.AddComponent<CanvasGroup>();
        dummyCG.interactable = false;
        dummyCG.blocksRaycasts = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dummyRect);
        dummyGridObj.SetActive(false);

        // YazÄ±larÄ±n animasyon sÄ±rasÄ±nda kullanÄ±lacak yedek kopyalarÄ±nÄ± yaratÄ±r
        if (Level1Text != null)
        {
            dummyLevel1Text = Instantiate(Level1Text, Level1Text.transform.parent);
            dummyText1Rect = dummyLevel1Text.GetComponent<RectTransform>();
            dummyLevel1Text.gameObject.SetActive(false);
        }
        if (Level2Text != null)
        {
            dummyLevel2Text = Instantiate(Level2Text, Level2Text.transform.parent);
            dummyText2Rect = dummyLevel2Text.GetComponent<RectTransform>();
            dummyLevel2Text.gameObject.SetActive(false);
        }
    }

    public void ShowWarningPanel(string chapterName)
    {
        if (warningPanelCG == null || warningPanelText == null) return;

      
        if (warningLocText == null) warningLocText = warningPanelText.GetComponent<LocalizedText>();
        if (warningLocText != null) warningLocText.enabled = false;

        
        string baseWarning = "HaritayÄ± bitirmeden geri dÃ¶nemezsin!";
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            baseWarning = LocalizationManager.Instance.currentData.warning_panel;
        }

        if (baseWarning.Contains("{MAP_NAME}"))
        {
            warningPanelText.text = baseWarning.Replace("{MAP_NAME}", chapterName);
        }
        else
        {
            warningPanelText.text = chapterName + "\n" + baseWarning;
        }

        StopAllCoroutines();
        StartCoroutine(FadeWarningPanelRoutine());
    }
    private System.Collections.IEnumerator FadeWarningPanelRoutine()
    {
        warningPanelCG.gameObject.SetActive(true);
        float t = 0;
        while (t < 0.4f) { t += Time.deltaTime; warningPanelCG.alpha = t / 0.4f; yield return null; }
        yield return new WaitForSeconds(1.2f);
        while (t > 0) { t -= Time.deltaTime; warningPanelCG.alpha = t / 0.4f; yield return null; }
        warningPanelCG.gameObject.SetActive(false);
    }

    // Ekran Ã§Ã¶zÃ¼nÃ¼rlÃ¼ÄŸÃ¼ne gÃ¶re GridLayoutGroup boÅŸluk boyutlarÄ±nÄ± dinamik olarak ayarlar
    private void AdjustGridLayout()
    {
        if (cachedGridLayout == null) cachedGridLayout = gridParent.GetComponent<GridLayoutGroup>();
        if (cachedGridLayout == null) return;
        GridLayoutGroup gridLayout = cachedGridLayout;

        if (!isGridInitialized)
        {
            originalCellSize = gridLayout.cellSize;
            originalSpacing = gridLayout.spacing;
            isGridInitialized = true;
        }

        if (cachedCanvasScaler != null)
        {
            CanvasScaler scaler = cachedCanvasScaler;
            if (scaler != null)
            {
                float currentAspect = (float)Screen.width / Screen.height;
                float referenceAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;
                
                // En-boy oranÄ±na gÃ¶re Ã§arpan hesapla
                float aspectMultiplier = currentAspect / referenceAspect;

                // Sadece yatay (X) boÅŸluklarÄ± ekrana gÃ¶re dinamik olarak oranla
                // HÃ¼cre boyutlarÄ± ve dikey (Y) boÅŸluklar orijinal kalacak
                gridLayout.cellSize = originalCellSize;
                gridLayout.spacing = new Vector2(originalSpacing.x * aspectMultiplier, originalSpacing.y);
            }
        }
    }

    void PrepareButtons()
    {
        foreach (Transform t in gridParent) Destroy(t.gameObject);
        spawnedButtons.Clear();

        for (int i = 0; i < levelsPerPage; i++)
        {
            GameObject btnObj = Instantiate(buttonPrefab, gridParent);
            LevelMenuButton script = btnObj.GetComponent<LevelMenuButton>();
            spawnedButtons.Add(script);
        }
    }

    void CreatePaginationDots()
    {
        foreach (Transform t in dotsParent) Destroy(t.gameObject);
        spawnedDots.Clear();
        int totalPages = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);
        for (int i = 0; i < totalPages; i++)
        {
            GameObject dot = Instantiate(dotPrefab, dotsParent);
            Image dotImage = dot.GetComponent<Image>();
            spawnedDots.Add(dotImage);
        }
    }
void UpdatePaginationDots()
    {
        for (int i = 0; i < spawnedDots.Count; i++)
        {
            bool isActive = (i == currentPage);
            spawnedDots[i].color = isActive ? activeColor : inactiveColor;
            
            // Aktif olan nokta 0.6f, pasif olanlar 0.8f olacak ÅŸekilde Ã¶lÃ§eklenir
            spawnedDots[i].transform.localScale = isActive ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0.6f, 0.6f, 1f);
        }
    }
    void RefreshPageUI()
    {
        UpdatePaginationDots();

        if (LocalizationManager.Instance == null || LocalizationManager.Instance.currentData == null) return;
        var data = LocalizationManager.Instance.currentData;
        if (data.page_titles == null) return;

        int baseIndex = currentPage * 2;
        if (baseIndex < data.page_titles.Length) Level1Text.text = data.page_titles[baseIndex];
        else Level1Text.text = "CHAPTER " + (currentPage + 1);

        if (Level2Text != null)
        {
            if (baseIndex + 1 < data.page_titles.Length) Level2Text.text = data.page_titles[baseIndex + 1];
            else Level2Text.text = "";
        }

        if (LocalizationManager.Instance.currentData.Level1 != null)
            Level1 = new List<string>(LocalizationManager.Instance.currentData.Level1);
    }

    void FillGridWithPageData(int pageIndex, List<LevelMenuButton> targetButtons)
    {
        int startIndex = pageIndex * levelsPerPage;

        for (int i = 0; i < targetButtons.Count; i++)
        {
            int currentDataIndex = startIndex + i;

            if (currentDataIndex < allGameLevels.Count)
            {
                targetButtons[i].gameObject.SetActive(true);
                LevelData data = allGameLevels[currentDataIndex];

                // BÃ¶lÃ¼m kilit ve tamamlanma verilerini PlayerPrefs'ten Ã§eker
                data.isUnlocked = PlayerPrefs.GetInt(Constants.PREF_LEVEL_UNLOCKED_PREFIX + data.levelID, data.levelID == 0 ? 1 : 0) == 1;
                data.isCompleted = PlayerPrefs.GetInt(Constants.PREF_LEVEL_COMPLETE_PREFIX + data.levelID, 0) == 1;

                bool isComingSoon = (pageIndex > 1);
                string localizedLevelName = data.levelName;

                // YerelleÅŸtirilmiÅŸ bÃ¶lÃ¼m isimlerini atar
                if (pageIndex == 0 && i < Level1.Count) localizedLevelName = Level1[i];
                else if (pageIndex == 1 && i < Level2.Count) localizedLevelName = Level2[i];

                targetButtons[i].Setup(currentDataIndex, data, isComingSoon, localizedLevelName);
            }
            else
            {
                targetButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void NextPage()
    {
        if (isAnimating) return;
        int totalPages = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);
        int targetPage = (currentPage >= totalPages - 1) ? 0 : currentPage + 1;
        StartCoroutine(AnimatePageChange(targetPage, -1));
    }

    public void PreviousPage()
    {
        if (isAnimating) return;
        int totalPages = Mathf.CeilToInt((float)allGameLevels.Count / levelsPerPage);
        int targetPage = (currentPage <= 0) ? totalPages - 1 : currentPage - 1;
        StartCoroutine(AnimatePageChange(targetPage, 1));
    }

    private System.Collections.IEnumerator FadeComingSoonRoutine(CanvasGroup cg, float startAlpha, float endAlpha, bool disableAfter)
    {
        if (cg == null || comingSoonPanel == null) yield break;

        comingSoonPanel.SetActive(true);
        cg.alpha = startAlpha;

        float t = 0;
        while (t < comingSoonFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(t / comingSoonFadeDuration));
            yield return null;
        }

        cg.alpha = endAlpha;
        if (disableAfter) comingSoonPanel.SetActive(false);
    }

    // Grid ve yazÄ±larÄ±n RectTransform interpolasyonu (Lerp) ile yumuÅŸak geÃ§iÅŸ yapmasÄ±nÄ± saÄŸlar
    private System.Collections.IEnumerator AnimatePageChange(int targetPage, int direction)
    {
        isAnimating = true;

        // Tam ekran geniÅŸliÄŸinde kaydÄ±rma yapabilmek iÃ§in Canvas'Ä±n gerÃ§ek geniÅŸliÄŸini alÄ±r
        if (cachedCanvasRect != null)
        {
            slideDistance = cachedCanvasRect.rect.width;
        }
        else
        {
            slideDistance = Screen.width;
        }

        Vector2 startPos = originalGridPos;
        Vector2 offsetOut = new Vector2(direction * slideDistance, 0);
        Vector2 offsetIn = new Vector2(-direction * slideDistance, 0);

        Vector2 outPos = startPos + offsetOut;
        Vector2 inPos = startPos + offsetIn;

        float textDirMult = reverseTextDirection ? -1f : 1f;

        Vector2 t1Out = origText1Pos + (offsetOut * textDirMult);
        Vector2 t1In = origText1Pos + (offsetIn * textDirMult);
        Vector2 t2Out = origText2Pos + (offsetOut * textDirMult);
        Vector2 t2In = origText2Pos + (offsetIn * textDirMult);

        // Arka plan geÃ§iÅŸi iÃ§in yedek gÃ¶rseli hazÄ±rlar
        if (backgroundImageBottom != null && pageBackgrounds != null && targetPage < pageBackgrounds.Count)
        {
            backgroundImageBottom.sprite = pageBackgrounds[targetPage];
            backgroundImageBottom.gameObject.SetActive(true);
            if (bgRectBottom != null) bgRectBottom.anchoredPosition = inPos;
        }

        // Eski verileri yedek (Dummy) tabloya ve yazÄ±ya aktarÄ±r
        dummyGridObj.SetActive(true);
        dummyRect.anchoredPosition = startPos;
        FillGridWithPageData(currentPage, dummyButtons);

        if (dummyLevel1Text != null) { dummyLevel1Text.text = Level1Text.text; dummyLevel1Text.gameObject.SetActive(true); dummyText1Rect.anchoredPosition = origText1Pos; }
        if (dummyLevel2Text != null) { dummyLevel2Text.text = Level2Text.text; dummyLevel2Text.gameObject.SetActive(true); dummyText2Rect.anchoredPosition = origText2Pos; }

        CanvasGroup csGroup = comingSoonCG;
        bool oldHasCS = false, newHasCS = false;
        if (comingSoonPanel != null)
        {
            oldHasCS = (currentPage > 1);
            newHasCS = (targetPage > 1);
        }

        // Yeni sayfa verilerini asÄ±l objelere yÃ¼kler
        currentPage = targetPage;
        PlayerPrefs.SetInt(Constants.PREF_LAST_LEVEL_ID, currentPage * levelsPerPage);
        PlayerPrefs.Save();

        RefreshPageUI();
        FillGridWithPageData(currentPage, spawnedButtons);

        // Yeni objeleri ekran dÄ±ÅŸÄ±ndaki baÅŸlangÄ±Ã§ konumuna alÄ±r
        if (gridRect != null) gridRect.anchoredPosition = inPos;
        if (text1Rect != null) text1Rect.anchoredPosition = t1In;
        if (text2Rect != null) text2Rect.anchoredPosition = t2In;

        if (comingSoonCoroutine != null) StopCoroutine(comingSoonCoroutine);
        if (csGroup != null)
        {
            if (oldHasCS && !newHasCS) comingSoonCoroutine = StartCoroutine(FadeComingSoonRoutine(csGroup, csGroup.alpha, 0f, true));
            else if (!oldHasCS && newHasCS) comingSoonCoroutine = StartCoroutine(FadeComingSoonRoutine(csGroup, 0f, 1f, false));
            else if (newHasCS) { comingSoonPanel.SetActive(true); csGroup.alpha = 1f; }
            else comingSoonPanel.SetActive(false);
        }

        // YumuÅŸak geÃ§iÅŸ (Ease-Out) animasyon dÃ¶ngÃ¼sÃ¼
        float t = 0;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(t / slideDuration);
            float curve = 1f - Mathf.Pow(1f - percent, 3f);

            // Eski sayfalar dÄ±ÅŸarÄ± kayar
            if (dummyRect != null) dummyRect.anchoredPosition = Vector2.Lerp(startPos, outPos, curve);
            if (bgRectTop != null) bgRectTop.anchoredPosition = Vector2.Lerp(startPos, outPos, curve);
            if (dummyText1Rect != null) dummyText1Rect.anchoredPosition = Vector2.Lerp(origText1Pos, t1Out, curve);
            if (dummyText2Rect != null) dummyText2Rect.anchoredPosition = Vector2.Lerp(origText2Pos, t2Out, curve);

            // Yeni sayfalar iÃ§eri girer
            if (gridRect != null) gridRect.anchoredPosition = Vector2.Lerp(inPos, startPos, curve);
            if (bgRectBottom != null) bgRectBottom.anchoredPosition = Vector2.Lerp(inPos, startPos, curve);
            if (text1Rect != null) text1Rect.anchoredPosition = Vector2.Lerp(t1In, origText1Pos, curve);
            if (text2Rect != null) text2Rect.anchoredPosition = Vector2.Lerp(t2In, origText2Pos, curve);

            yield return null;
        }

        // Animasyon bittiÄŸinde temizlik ve konum sabitleme iÅŸlemleri
        dummyGridObj.SetActive(false);
        if (gridRect != null) gridRect.anchoredPosition = startPos;

        if (dummyLevel1Text != null) dummyLevel1Text.gameObject.SetActive(false);
        if (dummyLevel2Text != null) dummyLevel2Text.gameObject.SetActive(false);
        if (text1Rect != null) text1Rect.anchoredPosition = origText1Pos;
        if (text2Rect != null) text2Rect.anchoredPosition = origText2Pos;

        if (backgroundImageTop != null && pageBackgrounds != null && currentPage < pageBackgrounds.Count)
            backgroundImageTop.sprite = pageBackgrounds[currentPage];

        if (bgRectTop != null) bgRectTop.anchoredPosition = startPos;
        if (backgroundImageBottom != null) backgroundImageBottom.gameObject.SetActive(false);

        isAnimating = false;
    }

    public void MainMenu()
    {
        if (LevelTransition.Instance != null) LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene(Constants.SCENE_MAIN_MENU); });
        else SceneManager.LoadScene(Constants.SCENE_MAIN_MENU);
    }
    public void StartFakeLoading(float duration, Action onComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(FakeLoadingRoutine(duration, onComplete));
    }
    private IEnumerator FakeLoadingRoutine(float duration, Action onComplete)
    {
        if (loadingAdPanel != null) loadingAdPanel.SetActive(true);


        _dotsCoroutine = StartCoroutine(AnimateDots());

        yield return new WaitForSeconds(duration);


        if (_dotsCoroutine != null) StopCoroutine(_dotsCoroutine);
        if (loadingAdPanel != null) loadingAdPanel.SetActive(false);


        onComplete?.Invoke();

    }
    private IEnumerator AnimateDots()
    {
        string baseText = "Reklam YÃ¼kleniyor"; // BurayÄ± istediÄŸin gibi deÄŸiÅŸtir
        int dotCount = 0;

        while (true)
        {
            dotCount++;
            if (dotCount > 5) dotCount = 0; // 5 noktadan sonra sÄ±fÄ±rla

            if (loadingText != null)
                loadingText.text = baseText + new string('.', dotCount);

            yield return new WaitForSeconds(0.4f); // NoktalarÄ±n hÄ±zÄ±
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            UnlockAllLevels();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartFakeLoading(5.0f, () => { Debug.Log("Test BaÅŸarÄ±yla Bitti!"); });
        }
    }

    private void UnlockAllLevels()
    {
        foreach (LevelData data in allGameLevels)
        {
            data.isUnlocked = true;
            PlayerPrefs.SetInt(Constants.PREF_LEVEL_UNLOCKED_PREFIX + data.levelID, 1);
        }
        PlayerPrefs.Save();

        RefreshPageUI();
        FillGridWithPageData(currentPage, spawnedButtons);

        Debug.Log("ğŸ› ï¸ GELÄ°ÅTÄ°RÄ°CÄ° HÄ°LESÄ°: BÃ¼tÃ¼n bÃ¶lÃ¼mlerin kilidi aÃ§Ä±ldÄ±!");
    }
}
