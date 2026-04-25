using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance;

    [Header("Cinematic Doors (Sinematik Kapılar)")]
    public RectTransform topPanel;    // Üst kapı paneli
    public RectTransform bottomPanel; // Alt kapı paneli
    public float doorSpeed = 0.5f;

    [Header("Settings (Ayarlar)")]
    public bool openDoorsOnStart = true;
    public TextMeshProUGUI levelText;

    public static bool isComingFromDoorTransition = false;
    public static bool isTransitioning = false;

    // YENİ AMELİYAT: Eylem Kuyruğu (Hafıza) Değişkenleri
    private bool isQueued = false;
    private System.Action queuedAction = null;

    private float closedYOffset = 0f;

    [Header("Sounds (Sesler)")]
    public AudioClip fadeSound; // ElevenLabs'ten indirdiğin geçiş sesi
    private void Awake()
    {
        // Önemli Düzeltme: Yeni sahne yüklendiğinde eski hafızayı ve kilidi sıfırla ki bug'da kalmasın
        Instance = this;
        isTransitioning = false;
        isQueued = false;
        queuedAction = null;

        SetupDoors();
        if (levelText != null) levelText.alpha = 0f;
    }

    private void SetupDoors()
    {
        if (topPanel == null || bottomPanel == null) return;

        float h = GetCanvasHeight();
        float w = GetCanvasWidth() + 500f;

        float doorHeight = h * 0.6f;
        topPanel.sizeDelta = new Vector2(w, doorHeight);
        bottomPanel.sizeDelta = new Vector2(w, doorHeight);

        closedYOffset = doorHeight / 2f;

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

    IEnumerator Start()
    {
        if (isComingFromDoorTransition || openDoorsOnStart)
        {
            isTransitioning = true;
            isComingFromDoorTransition = false;

            yield return new WaitForSeconds(0.2f);
            yield return OpenDoorsRoutine();

            isTransitioning = false;

            // YENİ AMELİYAT: Kapılar tamamen açıldı! Sırada bekleyen (spamlanmış) bir emir var mı?
            if (isQueued)
            {
                isQueued = false;
                FadeOut(queuedAction); // Hafızadaki emri anında çalıştır!
            }
        }
    }

    public void FadeOut(System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            // Adam kapı açılırken tuşa bastıysa, reddetme! Sadece hafızaya al.
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
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.GetComponent<RectTransform>().rect.height : Screen.height;
    }

    private float GetCanvasWidth()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.GetComponent<RectTransform>().rect.width : Screen.width;
    }

    /// <summary>
    /// Plays the transition sound at 30% volume.
    /// (Geçiş sesini %30 seviyesinde çalar.)
    /// </summary>
    private void PlayFadeSound()
    {
        if (fadeSound != null && SoundManager.instance != null && SoundManager.instance.sfxSource != null)
        {
            // Sesi direkt SoundManager'ın ana hoparlöründen %30 şiddetinde çaldırıyoruz
            SoundManager.instance.sfxSource.PlayOneShot(fadeSound, 0.3f);
        }
    }
}