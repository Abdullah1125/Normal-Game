using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance;

    [Header("Settings")]
    public bool fadeInOnStart = true;
    public Image fadeImage;
    public TextMeshProUGUI levelText;
    public float fadeDuration = 0.5f;

    [Header("Colors")]
    public Color fadeColor = Color.black;

    private RectTransform fadeRect;

  
    public enum SlideDirection { None, Up, Down }
    public static SlideDirection pendingSlideDirection = SlideDirection.None; 

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (fadeImage != null)
        {
            fadeRect = fadeImage.GetComponent<RectTransform>();

           
            if (pendingSlideDirection != SlideDirection.None)
            {
                fadeRect.anchoredPosition = Vector2.zero;
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
                fadeImage.raycastTarget = true;
            }
           
            else if (fadeInOnStart)
            {
                fadeRect.anchoredPosition = Vector2.zero;
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
                fadeImage.raycastTarget = true;
            }
            else
            {
                fadeRect.anchoredPosition = Vector2.zero;
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
                fadeImage.raycastTarget = false;
            }
        }

        if (levelText != null) levelText.alpha = 0f;
    }

    IEnumerator Start()
    {
       
        if (pendingSlideDirection == SlideDirection.Up)
        {
           
            yield return SlideRoutine(Vector2.zero, new Vector2(0, GetCanvasHeight()), null, false);
            pendingSlideDirection = SlideDirection.None;
        }
        else if (pendingSlideDirection == SlideDirection.Down)
        {
          
            yield return SlideRoutine(Vector2.zero, new Vector2(0, -GetCanvasHeight()), null, false);
            pendingSlideDirection = SlideDirection.None;
        }
        else if (fadeInOnStart)
        {
            yield return new WaitForSeconds(0.3f);
            FadeIn();
        }
    }

   
    public void SlideUpToScene(string sceneName)
    {
        pendingSlideDirection = SlideDirection.Up;
      
        StartCoroutine(SlideRoutine(new Vector2(0, -GetCanvasHeight()), Vector2.zero, () =>
        {
            SceneManager.LoadScene(sceneName);
        }, true));
    }

   
    public void SlideDownToScene(string sceneName)
    {
        pendingSlideDirection = SlideDirection.Down;
       
        StartCoroutine(SlideRoutine(new Vector2(0, GetCanvasHeight()), Vector2.zero, () =>
        {
            SceneManager.LoadScene(sceneName);
        }, true));
    }

    
    private IEnumerator SlideRoutine(Vector2 startPos, Vector2 endPos, System.Action onComplete, bool isClosing)
    {
        if (fadeImage == null) yield break;

        fadeImage.raycastTarget = true;
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        fadeRect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            t = t * t * (3f - 2f * t);

            fadeRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        fadeRect.anchoredPosition = endPos;
        fadeImage.raycastTarget = isClosing;
        onComplete?.Invoke();
    }

  
    public void FadeIn(System.Action onComplete = null) { StartCoroutine(FadeRoutine(1f, 0f, onComplete, false)); }
    public void FadeOut(System.Action onComplete = null) { StartCoroutine(FadeRoutine(0f, 1f, onComplete, true)); }

    public void DoTransition(string message, System.Action middleAction)
    {
        if (levelText != null) levelText.text = message;
        StartCoroutine(TransitionRoutine(middleAction));
    }

    public void DoTransition(System.Action middleAction)
    {
        if (levelText != null) levelText.text = "";
        StartCoroutine(TransitionRoutine(middleAction));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, System.Action onComplete, bool showText)
    {
        if (fadeImage == null) yield break;
        fadeRect.anchoredPosition = Vector2.zero;
        fadeImage.raycastTarget = true;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            if (levelText != null) levelText.alpha = alpha;
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, endAlpha);
        if (levelText != null) levelText.alpha = endAlpha;
        fadeImage.raycastTarget = endAlpha > 0.5f;
        onComplete?.Invoke();
    }

    private IEnumerator TransitionRoutine(System.Action middleAction)
    {
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        yield return FadeRoutine(0f, 1f, null, true);
        middleAction?.Invoke();
        yield return new WaitForSeconds(0.3f);
        yield return FadeRoutine(1f, 0f, null, false);
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;
    }

    private float GetCanvasHeight()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) return canvas.GetComponent<RectTransform>().rect.height;
        return Screen.height;
    }
}