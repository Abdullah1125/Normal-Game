using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelUIManager : MonoBehaviour
{
    public List<LevelData> allGameLevels;
    public GameObject buttonPrefab;
    public Transform gridParent;

    private int currentPage = 0;
    private int levelsPerPage = 12;
    private List<LevelMenuButton> spawnedButtons = new List<LevelMenuButton>();
    public GameObject comingSoonPanel;

    [Header("Page Backgrounds")]
    public Image backgroundImageTop;
    public RectTransform bgRectTop;
    public Image backgroundImageBottom;
    public RectTransform bgRectBottom;
    public List<Sprite> pageBackgrounds;

    [Header("Page Transition Settings")]
    public CanvasGroup gridCanvasGroup;
    public RectTransform gridRect;
    public float slideDistance = 1920f;
    public float slideDuration = 0.15f;

    [Header("Coming Soon Settings")]
    public float comingSoonFadeDuration = 0.4f;
    private Coroutine comingSoonCoroutine;

    private bool isAnimating = false;

    [Header("Pagination Settings")]
    public GameObject dotPrefab;
    public Transform dotsParent;
    public Color activeColor = Color.white;
    public Color inactiveColor = Color.gray;

    [Header("Page Titles")]
    public TMP_Text Level1Text;
    public TMP_Text Level2Text;

    [Header("Warning Settings")]
    public CanvasGroup warningPanelCG;

    private List<Image> spawnedDots = new List<Image>();

    public List<string> Level1 = new List<string>();
    public List<string> Level2 = new List<string>();

    void Start()
    {
        PrepareButtons();
        CreatePaginationDots();

        if (backgroundImageTop != null && pageBackgrounds != null && pageBackgrounds.Count > 0)
        {
            if (currentPage < pageBackgrounds.Count) backgroundImageTop.sprite = pageBackgrounds[currentPage];
            backgroundImageTop.gameObject.SetActive(true);
        }
        if (bgRectTop != null) bgRectTop.anchoredPosition = Vector2.zero;
        if (backgroundImageBottom != null) backgroundImageBottom.gameObject.SetActive(false);

        if (comingSoonPanel != null)
        {
            CanvasGroup cg = comingSoonPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = comingSoonPanel.AddComponent<CanvasGroup>();
            cg.alpha = (currentPage > 0) ? 1f : 0f;
            comingSoonPanel.SetActive(currentPage > 0);
        }

        RefreshPage();
    }

    public void ShowWarningPanel()
    {
        if (warningPanelCG == null) return;
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
            spawnedDots[i].color = (i == currentPage) ? activeColor : inactiveColor;
        }
    }

    void UpdateTexts()
    {
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
    }

    public void RefreshPage()
    {
        UpdateTexts();
        UpdatePaginationDots();

        int startIndex = currentPage * levelsPerPage;

        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            if (LocalizationManager.Instance.currentData.Level1 != null)
                Level1 = new List<string>(LocalizationManager.Instance.currentData.Level1);
        }

        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            int currentDataIndex = startIndex + i;

            if (currentDataIndex < allGameLevels.Count)
            {
                spawnedButtons[i].gameObject.SetActive(true);
                LevelData data = allGameLevels[currentDataIndex];

                data.isUnlocked = PlayerPrefs.GetInt("LevelUnlocked_" + data.levelID, data.levelID == 0 ? 1 : 0) == 1;
                data.isCompleted = PlayerPrefs.GetInt("LevelComplete_" + data.levelID, 0) == 1;

                bool isComingSoon = (currentPage > 0);
                string localizedLevelName = data.levelName;

                if (currentPage == 0 && i < Level1.Count) localizedLevelName = Level1[i];
                else if (currentPage == 1 && i < Level2.Count) localizedLevelName = Level2[i];

                spawnedButtons[i].Setup(currentDataIndex, data, isComingSoon, localizedLevelName);
            }
            else
            {
                spawnedButtons[i].gameObject.SetActive(false);
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

    // YENÝ: Kasýp atlamayý (Lag Spike) engelleyen sarsýlmaz Erime Motoru
    private System.Collections.IEnumerator FadeComingSoonRoutine(CanvasGroup cg, float startAlpha, float endAlpha, bool disableAfter)
    {
        if (cg == null || comingSoonPanel == null) yield break;

        comingSoonPanel.SetActive(true);
        cg.alpha = startAlpha;

        yield return null; // KASMAYI EMÝCÝ SÝGORTA: Ýlk aðýr frame'in zaman atlamasýný çöpe atýyoruz!

        float t = 0;
        while (t < comingSoonFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            // CLAMP01: Zamanýn taþtýðý o son saniyelerde oranýn bozulmasýný engeller
            float percent = Mathf.Clamp01(t / comingSoonFadeDuration);
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, percent);
            yield return null;
        }

        cg.alpha = endAlpha;

        if (disableAfter)
        {
            comingSoonPanel.SetActive(false);
        }
    }

    private System.Collections.IEnumerator AnimatePageChange(int targetPage, int direction)
    {
        isAnimating = true;

        Vector2 startPos = Vector2.zero;
        Vector2 outPos = new Vector2(direction * slideDistance, 0);
        Vector2 inPos = new Vector2(-direction * slideDistance, 0);

        if (backgroundImageBottom != null && pageBackgrounds != null && targetPage < pageBackgrounds.Count)
        {
            backgroundImageBottom.sprite = pageBackgrounds[targetPage];
            backgroundImageBottom.gameObject.SetActive(true);
            if (bgRectBottom != null) bgRectBottom.anchoredPosition = inPos;
        }

        GameObject dummyGridObj = Instantiate(gridParent.gameObject, gridParent.parent);
        RectTransform dummyRect = dummyGridObj.GetComponent<RectTransform>();

        dummyRect.anchorMin = gridRect.anchorMin;
        dummyRect.anchorMax = gridRect.anchorMax;
        dummyRect.pivot = gridRect.pivot;
        dummyRect.sizeDelta = gridRect.sizeDelta;
        dummyGridObj.transform.localScale = gridRect.localScale;
        dummyRect.anchoredPosition = startPos;

        LayoutRebuilder.ForceRebuildLayoutImmediate(dummyRect);

        CanvasGroup dummyCG = dummyGridObj.GetComponent<CanvasGroup>();
        if (dummyCG == null) dummyCG = dummyGridObj.AddComponent<CanvasGroup>();
        dummyCG.interactable = false;
        dummyCG.blocksRaycasts = false;

        CanvasGroup csGroup = null;
        bool oldHasCS = false, newHasCS = false;
        if (comingSoonPanel != null)
        {
            csGroup = comingSoonPanel.GetComponent<CanvasGroup>();
            if (csGroup == null) csGroup = comingSoonPanel.AddComponent<CanvasGroup>();
            oldHasCS = (currentPage > 0);
            newHasCS = (targetPage > 0);
        }

        currentPage = targetPage;
        RefreshPage();

        // --- BAÐIMSIZ FADE BAÞLATICI ---
        if (comingSoonCoroutine != null) StopCoroutine(comingSoonCoroutine);

        if (csGroup != null)
        {
            if (oldHasCS && !newHasCS)
            {
                comingSoonCoroutine = StartCoroutine(FadeComingSoonRoutine(csGroup, csGroup.alpha, 0f, true));
            }
            else if (!oldHasCS && newHasCS)
            {
                comingSoonCoroutine = StartCoroutine(FadeComingSoonRoutine(csGroup, 0f, 1f, false));
            }
            else if (newHasCS)
            {
                comingSoonPanel.SetActive(true);
                csGroup.alpha = 1f;
            }
            else
            {
                comingSoonPanel.SetActive(false);
            }
        }

        if (gridRect != null) gridRect.anchoredPosition = inPos;
        if (gridRect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);

        yield return null; // KASMAYI EMÝCÝ SÝGORTA 2: Butonlarýn kaymasý da kasmadan pürüzsüz baþlasýn

        float t = 0;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float percent = Mathf.Clamp01(t / slideDuration);

            float curve = 1f - Mathf.Pow(1f - percent, 3f);

            if (dummyRect != null) dummyRect.anchoredPosition = Vector2.Lerp(startPos, outPos, curve);
            if (bgRectTop != null) bgRectTop.anchoredPosition = Vector2.Lerp(startPos, outPos, curve);

            if (gridRect != null) gridRect.anchoredPosition = Vector2.Lerp(inPos, startPos, curve);
            if (bgRectBottom != null) bgRectBottom.anchoredPosition = Vector2.Lerp(inPos, startPos, curve);

            yield return null;
        }

        Destroy(dummyGridObj);
        if (gridRect != null) gridRect.anchoredPosition = startPos;

        if (backgroundImageTop != null && pageBackgrounds != null && currentPage < pageBackgrounds.Count)
        {
            backgroundImageTop.sprite = pageBackgrounds[currentPage];
        }
        if (bgRectTop != null) bgRectTop.anchoredPosition = startPos;
        if (backgroundImageBottom != null) backgroundImageBottom.gameObject.SetActive(false);

        isAnimating = false;
    }

    public void MainMenu()
    {
        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.FadeOut(() => { SceneManager.LoadScene("MainMenu"); });
        }
        else SceneManager.LoadScene("MainMenu");
    }
}