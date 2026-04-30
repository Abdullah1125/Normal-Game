using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class MenuBounceAnimator : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Animation Type (Animasyon Türü)")]
    public bool slideFromBottom = true;

    [Header("Delay Settings (Gecikme Ayarları)")]
    public float openDelay = 0f;
    public float closeDelay = 0f;

    [Header("Opening Settings (Açılma Ayarları)")]
    public float openDuration = 0.4f;
    public float openOvershoot = 1.5f;

    [Header("Shutdown Settings (Kapanma Ayarları)")]
    public float closeDuration = 0.3f;
    public float closeAnticipation = 1.5f;

    [Header("Glide Settings (Kayma Ayarları)")]
    public float startYOffset = -1500f;

    private Vector2 originalPosition;
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        Vector2 offset = slideFromBottom ? new Vector2(0, startYOffset) : Vector2.zero;
        rectTransform.anchoredPosition = originalPosition + offset;
        rectTransform.localScale = slideFromBottom ? originalScale * 0.5f : Vector3.zero;

        Canvas.ForceUpdateCanvases();

        StopAllCoroutines();
        StartCoroutine(BounceRoutine(true));
    }

    public void CloseMenu()
    {
        StopAllCoroutines();
        StartCoroutine(BounceRoutine(false));
    }

    /// <summary>
    /// Animates the menu and triggers sounds for both opening and closing.
    /// (Menüyü canlandırır ve hem açılış hem kapanış için sesleri tetikler.)
    /// </summary>
    private IEnumerator BounceRoutine(bool isOpening)
    {
        // Sound Logic (Ses Mantığı)
        if (SoundManager.Instance != null)
        {
            if (isOpening)
            {
                // Açılış sesi (Aşağıdan mı ortadan mı?)
                if (slideFromBottom)
                    SoundManager.PlayThemeSFX(SFXType.MenuSlide, 0.2f);
                else
                    SoundManager.PlayThemeSFX(SFXType.MenuPop, 0.2f);
            }
            else
            {
                // Kapanış sesi (Burayı boş bırakmıştın, ekledik!)
                SoundManager.PlayThemeSFX(SFXType.MenuSlide, 0.1f);
            }
        }

        float delay = isOpening ? openDelay : closeDelay;
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        float elapsed = 0f;
        float duration = isOpening ? openDuration : closeDuration;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScl = rectTransform.localScale;
        Vector2 offset = slideFromBottom ? new Vector2(0, startYOffset) : Vector2.zero;
        Vector2 endPos = isOpening ? originalPosition : originalPosition + offset;
        Vector3 endScl = isOpening ? originalScale : Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float scaleCurve;
            float posCurve;

            if (isOpening)
            {
                float s = openOvershoot;
                float tempT = t - 1.0f;
                scaleCurve = tempT * tempT * ((s + 1) * tempT + s) + 1.0f;
                posCurve = 1f - Mathf.Pow(1f - t, 3f);
            }
            else
            {
                float s = closeAnticipation;
                scaleCurve = t * t * ((s + 1) * t - s);
                posCurve = t * t * t;
            }

            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, posCurve);
            rectTransform.localScale = Vector3.LerpUnclamped(startScl, endScl, scaleCurve);

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        rectTransform.localScale = endScl;

        if (!isOpening) gameObject.SetActive(false);
    }
}
