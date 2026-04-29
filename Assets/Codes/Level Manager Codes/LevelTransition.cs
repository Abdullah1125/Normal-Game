using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Manages cinematic door transitions and scene loading.
/// (Sinematik kapı geçişlerini ve sahne yüklemelerini yönetir.)
/// </summary>
public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance { get; private set; }

    [Header("Cinematic Doors (Sinematik Kapılar)")]
    public RectTransform topPanel;    // Üst kapı paneli
    public RectTransform bottomPanel; // Alt kapı paneli
    public float doorSpeed = 0.5f;

    [Header("Settings (Ayarlar)")]
    public bool openDoorsOnStart = true;
    public float overlapMargin = 10f; // Kapanmada ortadaki boşluğu kapatmak için kesişme payı
    public TextMeshProUGUI levelText;

    // Sahneler arası taşınan statik durum değişkenleri
    public static bool isComingFromDoorTransition = false;
    public static bool isTransitioning = false;

    // Eylem Kuyruğu (Hafıza) Değişkenleri
    private bool isQueued = false;
    private System.Action queuedAction = null;

    private float closedYOffset = 0f;

    [Header("Sounds (Sesler)")]
    public AudioClip fadeSound;

    // Önbellek değişkenleri
    private RectTransform canvasRect;

    /// <summary>
    /// Initializes singleton, resets states, and caches components.
    /// (Singleton'ı başlatır, durumları sıfırlar ve bileşenleri önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        isTransitioning = false;
        isQueued = false;
        queuedAction = null;

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            canvasRect = rootCanvas.GetComponent<RectTransform>();
        }

        Canvas.ForceUpdateCanvases();

        SetupDoors();
        if (levelText != null) levelText.alpha = 0f;
    }

    /// <summary>
    /// Automatically adjusts doors when screen resolution or orientation changes.
    /// (Ekran çözünürlüğü veya yönelimi değiştiğinde kapıları otomatik ayarlar.)
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        if (canvasRect != null && !isTransitioning)
        {
            SetupDoors();
        }
    }

    /// <summary>
    /// Adjusts door sizes and initial positions based on screen dimensions.
    /// (Kapı boyutlarını ve başlangıç pozisyonlarını ekran boyutlarına göre ayarlar.)
    /// </summary>
    private void SetupDoors()
    {
        if (topPanel == null || bottomPanel == null) return;

        topPanel.anchorMin = new Vector2(0.5f, 0.5f);
        topPanel.anchorMax = new Vector2(0.5f, 0.5f);
        topPanel.pivot = new Vector2(0.5f, 0.5f);

        bottomPanel.anchorMin = new Vector2(0.5f, 0.5f);
        bottomPanel.anchorMax = new Vector2(0.5f, 0.5f);
        bottomPanel.pivot = new Vector2(0.5f, 0.5f);

        float h = GetCanvasHeight();
        float w = GetCanvasWidth() + 500f;

        float doorHeight = h * 0.6f;
        topPanel.sizeDelta = new Vector2(w, doorHeight);
        bottomPanel.sizeDelta = new Vector2(w, doorHeight);

        // Kesişme payını merkeze itme gücünden çıkarıyoruz ki paneller merkezin biraz daha ötesine geçsin
        closedYOffset = (doorHeight / 2f) - overlapMargin;

        if (isComingFromDoorTransition || openDoorsOnStart)
        {
            topPanel.gameObject.SetActive(true);
            bottomPanel.gameObject.SetActive(true);

            topPanel.anchoredPosition = new Vector2(0, closedYOffset);
            bottomPanel.anchoredPosition = new Vector2(0, -closedYOffset);
        }
        else
        {
            topPanel.gameObject.SetActive(false);
            bottomPanel.gameObject.SetActive(false);

            topPanel.anchoredPosition = new Vector2(0, h + closedYOffset);
            bottomPanel.anchoredPosition = new Vector2(0, -(h + closedYOffset));
        }
    }

    /// <summary>
    /// Opens the doors at the start of the scene if requested.
    /// (İsteniyorsa sahne başlangıcında kapıları açar.)
    /// </summary>
    private IEnumerator Start()
    {
        yield return null;

        if (isComingFromDoorTransition || openDoorsOnStart)
        {
            isTransitioning = true;
            isComingFromDoorTransition = false;

            yield return new WaitForSeconds(0.2f);
            yield return OpenDoorsRoutine();

            isTransitioning = false;

            if (isQueued)
            {
                isQueued = false;
                FadeOut(queuedAction);
            }
        }
    }

    /// <summary>
    /// Closes doors and executes the provided action upon completion.
    /// (Kapıları kapatır ve tamamlandığında belirtilen eylemi çalıştırır.)
    /// </summary>
    public void FadeOut(System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            isQueued = true;
            queuedAction = onComplete;
            return;
        }

        isTransitioning = true;
        PlayFadeSound();

        StartCoroutine(CloseDoorsRoutine(() =>
        {
            isComingFromDoorTransition = true;
            onComplete?.Invoke();
        }));
    }

    public void DoTransition(System.Action middleAction)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine("", middleAction));
    }

    public void DoTransition(string message, System.Action middleAction)
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(message, middleAction));
    }

    private IEnumerator TransitionRoutine(string message, System.Action middleAction)
    {
        isTransitioning = true;
        PlayFadeSound();

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        if (levelText != null) levelText.text = message;

        yield return CloseDoorsRoutine(null);

        if (levelText != null) levelText.alpha = 1f;
        middleAction?.Invoke();
        yield return new WaitForSeconds(0.4f);

        if (levelText != null) levelText.alpha = 0f;
        yield return OpenDoorsRoutine();

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;

        isTransitioning = false;
    }

    private IEnumerator CloseDoorsRoutine(System.Action onComplete)
    {
        float h = GetCanvasHeight();

        if (topPanel != null) topPanel.gameObject.SetActive(true);
        if (bottomPanel != null) bottomPanel.gameObject.SetActive(true);

        Vector2 tStart = new Vector2(0, h + closedYOffset);
        Vector2 tEnd = new Vector2(0, closedYOffset);
        Vector2 bStart = new Vector2(0, -(h + closedYOffset));
        Vector2 bEnd = new Vector2(0, -closedYOffset);

        yield return MoveDoors(tStart, tEnd, bStart, bEnd);
        onComplete?.Invoke();
    }

    private IEnumerator OpenDoorsRoutine()
    {
        float h = GetCanvasHeight();

        Vector2 tStart = new Vector2(0, closedYOffset);
        Vector2 tEnd = new Vector2(0, h + closedYOffset);
        Vector2 bStart = new Vector2(0, -closedYOffset);
        Vector2 bEnd = new Vector2(0, -(h + closedYOffset));

        yield return MoveDoors(tStart, tEnd, bStart, bEnd);

        if (topPanel != null) topPanel.gameObject.SetActive(false);
        if (bottomPanel != null) bottomPanel.gameObject.SetActive(false);
    }

    private IEnumerator MoveDoors(Vector2 tStart, Vector2 tEnd, Vector2 bStart, Vector2 bEnd)
    {
        float elapsed = 0f;

        while (elapsed < doorSpeed)
        {
            float safeDeltaTime = Time.unscaledDeltaTime;
            if (safeDeltaTime > 0.05f) safeDeltaTime = 0.05f;

            elapsed += safeDeltaTime;
            float t = elapsed / doorSpeed;
            t = Mathf.Clamp01(t);
            t = t * t * (3f - 2f * t);

            if (topPanel != null) topPanel.anchoredPosition = Vector2.Lerp(tStart, tEnd, t);
            if (bottomPanel != null) bottomPanel.anchoredPosition = Vector2.Lerp(bStart, bEnd, t);

            yield return null;
        }

        if (topPanel != null) topPanel.anchoredPosition = tEnd;
        if (bottomPanel != null) bottomPanel.anchoredPosition = bEnd;
    }

    private float GetCanvasHeight()
    {
        return canvasRect != null ? canvasRect.rect.height : Screen.height;
    }

    private float GetCanvasWidth()
    {
        return canvasRect != null ? canvasRect.rect.width : Screen.width;
    }

    private void PlayFadeSound()
    {
        if (fadeSound != null && SoundManager.instance != null && SoundManager.instance.sfxSource != null)
        {
            SoundManager.instance.sfxSource.PlayOneShot(fadeSound, 0.3f);
        }
    }
}