using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; 

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance;

    [Header("Ayarlar")]
    public Image fadeImage;
    public TextMeshProUGUI levelText; 
    public float fadeDuration = 0.5f;

    [Header("Renkler")]
    public Color fadeColor = Color.black;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Baþlangýçta þeffaf
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.raycastTarget = false;
        }

        // Baþlangýçta yazýyý gizle
        if (levelText != null) levelText.alpha = 0f;
    }

    void Start()
    {
        FadeIn();
    }

    public void FadeIn(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(1f, 0f, onComplete, false));
    }

    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(0f, 1f, onComplete, true));
    }

    // Yazý gönderilebilen geçiþ fonksiyonu
    public void DoTransition(string message, System.Action middleAction)
    {
        if (levelText != null) levelText.text = message;
        StartCoroutine(TransitionRoutine(middleAction));
    }

    // ESKÝ: Sadece aksiyon alan orijinal fonksiyon.
    public void DoTransition(System.Action middleAction)
    {
        if (levelText != null) levelText.text = ""; 
        StartCoroutine(TransitionRoutine(middleAction));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, System.Action onComplete, bool showText)
    {
        if (fadeImage == null) yield break;

        fadeImage.raycastTarget = true;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);

           
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);

            // Yazýyý da panel ile ayný oranda göster/gizle
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
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = false;
        }

        // Ekraný kapatýrken yazýyý da göster
        yield return FadeRoutine(0f, 1f, null, true);

        // Ortadaki iþi yap (Level deðiþimi vb.)
        middleAction?.Invoke();

        // Yazýnýn okunmasý için minik bir ekstra bekleme
        yield return new WaitForSeconds(0.3f);

        // Ekraný açarken her þeyi gizle
        yield return FadeRoutine(1f, 0f, null, false);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = true;
        }
    }
}