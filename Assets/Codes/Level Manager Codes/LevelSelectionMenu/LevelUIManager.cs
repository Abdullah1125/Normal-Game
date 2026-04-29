using System;
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

    [Header("Page Backgrounds(Sayfa Arka Planları)")]
    public Image backgroundImageTop;
    public RectTransform bgRectTop;
    public Image backgroundImageBottom;
    public RectTransform bgRectBottom;
    public List<Sprite> pageBackgrounds;

    [Header("Page Transition Settings(Sayfa Geçiş Ayarları)")]
    public CanvasGroup gridCanvasGroup;
    public RectTransform gridRect;
    public float slideDistance = 1920f;
    public float slideDuration = 0.15f;

    [Header("Coming Soon Settings(Çok Yakında Ayarları)")]
    public float comingSoonFadeDuration = 0.4f;
    private Coroutine comingSoonCoroutine;

    private bool isAnimating = false;

    [Header("Text Settings(Yazı Ayarları)")]
    public bool reverseTextDirection = false;

    private TMP_Text dummyLevel1Text;
    private TMP_Text dummyLevel2Text;
    private RectTransform text1Rect;
    private RectTransform text2Rect;
    private RectTransform dummyText1Rect;
    private RectTransform dummyText2Rect;
    private Vector2 origText1Pos;
    private Vector2 origText2Pos;

    [Header("Pagination Settings(Sayfalandırma Ayarları)")]
    public GameObject dotPrefab;
    public Transform dotsParent;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Page Titles(Sayfa Başlıkları)")]
    public TMP_Text Level1Text;
    public TMP_Text Level2Text;

    [Header("Warning Settings(Uyarı Ayaları)")]
    public CanvasGroup warningPanelCG;
    public TextMeshProUGUI warningPanelText;

    [Header("Grid Layout Settings(Izgara Boyut Ayarları)")]
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

    private Coroutine _dotsCoroutine;
    void Start()
    {
        if (gridRect == null) gridRect = gridParent.GetComponent<RectTransform>();

        // Grid'in editördeki orijinal pozisyonunu hafızaya alır
        originalGridPos = gridRect.anchoredPosition;

        // Tam ekran genişliğinde kaydırma yapabilmek için Canvas'ın gerçek genişliğini alır
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas != null)
        {
            RectTransform canvasRect = canvas.rootCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                slideDistance = canvasRect.rect.width;
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

        // Arka plan görsellerini ve ilk sayfa pozisyonlarını ayarlar
        if (backgroundImageTop != null && pageBackgrounds != null && pageBackgrounds.Count > 0)
        {
            if (currentPage < pageBackgrounds.Count) backgroundImageTop.sprite = pageBackgrounds[currentPage];
            backgroundImageTop.gameObject.SetActive(true);
        }
        if (bgRectTop != null) bgRectTop.anchoredPosition = originalGridPos;
        if (backgroundImageBottom != null) backgroundImageBottom.gameObject.SetActive(false);

        // Başlangıç sayfasının kilit (Coming Soon) durumunu kontrol eder
        if (comingSoonPanel != null)
        {
            CanvasGroup cg = comingSoonPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = comingSoonPanel.AddComponent<CanvasGroup>();
            cg.alpha = (currentPage > 1) ? 1f : 0f;
            comingSoonPanel.SetActive(currentPage > 1);
        }

        RefreshPageUI();
        FillGridWithPageData(currentPage, spawnedButtons);
    }

    
    // Animasyonlarda render yükünü azaltmak için yedek tablo ve yazı kopyalarını oluşturur
    void CreateDummyPool()
    {
        dummyGridObj = Instantiate(gridParent.gameObject, gridParent.parent);
        dummyGridObj.name = "Optimized_DummyGrid";
        dummyRect = dummyGridObj.GetComponent<RectTransform>();

        // Kopyanın anchor ve pivot ayarlarını orijinaliyle eşitler
        dummyRect.anchorMin = gridRect.anchorMin;
        dummyRect.anchorMax = gridRect.anchorMax;
        dummyRect.pivot = gridRect.pivot;
        dummyRect.sizeDelta = gridRect.sizeDelta;
        dummyGridObj.transform.localScale = gridRect.localScale;

        // Yazıların RectTransform referanslarını ve orijinal konumlarını saklar
        text1Rect = Level1Text != null ? Level1Text.GetComponent<RectTransform>() : null;
        text2Rect = Level2Text != null ? Level2Text.GetComponent<RectTransform>() : null;
        if (text1Rect != null) origText1Pos = text1Rect.anchoredPosition;
        if (text2Rect != null) origText2Pos = text2Rect.anchoredPosition;

        // Havuz nesnesinin içini temizleyip yeni butonlar ekler
        foreach (Transform t in dummyGridObj.transform) Destroy(t.gameObject);
        for (int i = 0; i < levelsPerPage; i++)
        {
            GameObject btnObj = Instantiate(buttonPrefab, dummyGridObj.transform);
            dummyButtons.Add(btnObj.GetComponent<LevelMenuButton>());
        }

        // Havuz nesnesinin tıklanmasını engeller
        CanvasGroup dummyCG = dummyGridObj.GetComponent<CanvasGroup>();
        if (dummyCG == null) dummyCG = dummyGridObj.AddComponent<CanvasGroup>();
        dummyCG.interactable = false;
        dummyCG.blocksRaycasts = false;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dummyRect);
        dummyGridObj.SetActive(false);

        // Yazıların animasyon sırasında kullanılacak yedek kopyalarını yaratır
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

      
        LocalizedText locText = warningPanelText.GetComponent<LocalizedText>();
        if (locText != null) locText.enabled = false;

        
        string baseWarning = "Haritayı bitirmeden geri dönemezsin!";
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            baseWarning = LocalizationManager.Instance.currentData.warning_panel;
        }

       
        warningPanelText.text = chapterName + "\n" + baseWarning;

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

    // Ekran çözünürlüğüne göre GridLayoutGroup boşluk boyutlarını dinamik olarak ayarlar
    private void AdjustGridLayout()
    {
        GridLayoutGroup gridLayout = gridParent.GetComponent<GridLayoutGroup>();
        if (gridLayout == null) return;

        if (!isGridInitialized)
        {
            originalCellSize = gridLayout.cellSize;
            originalSpacing = gridLayout.spacing;
            isGridInitialized = true;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.rootCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                float currentAspect = (float)Screen.width / Screen.height;
                float referenceAspect = scaler.referenceResolution.x / scaler.referenceResolution.y;
                
                // En-boy oranına göre çarpan hesapla
                float aspectMultiplier = currentAspect / referenceAspect;

                // Sadece yatay (X) boşlukları ekrana göre dinamik olarak oranla
                // Hücre boyutları ve dikey (Y) boşluklar orijinal kalacak
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
            
            // Aktif olan nokta 0.6f, pasif olanlar 0.8f olacak şekilde ölçeklenir
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

                // Bölüm kilit ve tamamlanma verilerini PlayerPrefs'ten çeker
                data.isUnlocked = PlayerPrefs.GetInt("LevelUnlocked_" + data.levelID, data.levelID == 0 ? 1 : 0) == 1;
                data.isCompleted = PlayerPrefs.GetInt("LevelComplete_" + data.levelID, 0) == 1;

                bool isComingSoon = (pageIndex > 1);
                string localizedLevelName = data.levelName;

                // Yerelleştirilmiş bölüm isimlerini atar
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

    // Grid ve yazıların RectTransform interpolasyonu (Lerp) ile yumuşak geçiş yapmasını sağlar
    private System.Collections.IEnumerator AnimatePageChange(int targetPage, int direction)
    {
        isAnimating = true;

        // Tam ekran genişliğinde kaydırma yapabilmek için Canvas'ın gerçek genişliğini alır
        Canvas canvas = gridRect != null ? gridRect.GetComponentInParent<Canvas>() : null;
        if (canvas != null && canvas.rootCanvas != null)
        {
            RectTransform canvasRect = canvas.rootCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                slideDistance = canvasRect.rect.width;
            }
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

        // Arka plan geçişi için yedek görseli hazırlar
        if (backgroundImageBottom != null && pageBackgrounds != null && targetPage < pageBackgrounds.Count)
        {
            backgroundImageBottom.sprite = pageBackgrounds[targetPage];
            backgroundImageBottom.gameObject.SetActive(true);
            if (bgRectBottom != null) bgRectBottom.anchoredPosition = inPos;
        }

        // Eski verileri yedek (Dummy) tabloya ve yazıya aktarır
        dummyGridObj.SetActive(true);
        dummyRect.anchoredPosition = startPos;
        FillGridWithPageData(currentPage, dummyButtons);

        if (dummyLevel1Text != null) { dummyLevel1Text.text = Level1Text.text; dummyLevel1Text.gameObject.SetActive(true); dummyText1Rect.anchoredPosition = origText1Pos; }
        if (dummyLevel2Text != null) { dummyLevel2Text.text = Level2Text.text; dummyLevel2Text.gameObject.SetActive(true); dummyText2Rect.anchoredPosition = origText2Pos; }

        CanvasGroup csGroup = null;
        bool oldHasCS = false, newHasCS = false;
        if (comingSoonPanel != null)
        {
            csGroup = comingSoonPanel.GetComponent<CanvasGroup>();
            oldHasCS = (currentPage > 1);
            newHasCS = (targetPage > 1);
        }

        // Yeni sayfa verilerini asıl objelere yükler
        currentPage = targetPage;
        RefreshPageUI();
        FillGridWithPageData(currentPage, spawnedButtons);

        // Yeni objeleri ekran dışındaki başlangıç konumuna alır
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

        // Yumuşak geçiş (Ease-Out) animasyon döngüsü
        float t = 0;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(t / slideDuration);
            float curve = 1f - Mathf.Pow(1f - percent, 3f);

            // Eski sayfalar dışarı kayar
            if (dummyRect != null) dummyRect.anchoredPosition = Vector2.Lerp(startPos, outPos, curve);
            if (bgRectTop != null) bgRectTop.anchoredPosition = Vector2.Lerp(startPos, outPos, curve);
            if (dummyText1Rect != null) dummyText1Rect.anchoredPosition = Vector2.Lerp(origText1Pos, t1Out, curve);
            if (dummyText2Rect != null) dummyText2Rect.anchoredPosition = Vector2.Lerp(origText2Pos, t2Out, curve);

            // Yeni sayfalar içeri girer
            if (gridRect != null) gridRect.anchoredPosition = Vector2.Lerp(inPos, startPos, curve);
            if (bgRectBottom != null) bgRectBottom.anchoredPosition = Vector2.Lerp(inPos, startPos, curve);
            if (text1Rect != null) text1Rect.anchoredPosition = Vector2.Lerp(t1In, origText1Pos, curve);
            if (text2Rect != null) text2Rect.anchoredPosition = Vector2.Lerp(t2In, origText2Pos, curve);

            yield return null;
        }

        // Animasyon bittiğinde temizlik ve konum sabitleme işlemleri
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
        if (LevelTransition.Instance != null) LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene("MainMenu"); });
        else SceneManager.LoadScene("MainMenu");
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
        string baseText = "Reklam Yükleniyor"; // Burayı istediğin gibi değiştir
        int dotCount = 0;

        while (true)
        {
            dotCount++;
            if (dotCount > 5) dotCount = 0; // 5 noktadan sonra sıfırla

            if (loadingText != null)
                loadingText.text = baseText + new string('.', dotCount);

            yield return new WaitForSeconds(0.4f); // Noktaların hızı
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
            StartFakeLoading(5.0f, () => { Debug.Log("Test Başarıyla Bitti!"); });
        }
    }

    private void UnlockAllLevels()
    {
        foreach (LevelData data in allGameLevels)
        {
            data.isUnlocked = true;
            PlayerPrefs.SetInt("LevelUnlocked_" + data.levelID, 1);
        }
        PlayerPrefs.Save();

        RefreshPageUI();
        FillGridWithPageData(currentPage, spawnedButtons);

        Debug.Log("🛠️ GELİŞTİRİCİ HİLESİ: Bütün bölümlerin kilidi açıldı!");
    }
}