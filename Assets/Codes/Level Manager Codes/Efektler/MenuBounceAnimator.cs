using UnityEngine;
using System.Collections;

// Bu scriptin eklendiði objede RectTransform olmak zorunda, yoksa Unity otomatik ekler.
[RequireComponent(typeof(RectTransform))]
public class MenuBounceAnimator : MonoBehaviour
{
    // Panelin pozisyon ve boyut ayarlarýný deðiþtirmek için referansýmýz
    private RectTransform rectTransform;

    [Header("Animation Type(Animasyon Türü)")]
    // Tik açýksa aþaðýdan kayarak gelir. Tik kapalýysa olduðu yerde büyüyüp küçülür (Pop-up)
    public bool slideFromBottom = true;

    [Header("Delay Settings (Gecikme Ayarlarý)")]
    public float openDelay = 0f;    // Açýlmadan önce kaç saniye beklesin? (Pause menüsünün kapanmasýný beklemek için)
    public float closeDelay = 0f;   // Kapanmadan önce kaç saniye beklesin?

    [Header("Opening Settings(Açýlma Ayarlarý)")]
    public float openDuration = 0.4f;   // Menünün açýlma (ekrana gelme veya büyüme) süresi
    public float openOvershoot = 1.5f;  // Açýlýrken hedefi ne kadar aþýp (þiþip/zýplayýp) geri dönecek

    [Header("Shutdown Settings(Kapanma Ayarlarý)")]
    public float closeDuration = 0.3f;  // Menünün kapanma süresi
    // Kapanýrken önce esneme/þiþme þiddeti. Dümdüz küçülerek sönmesini istersen bunu Inspector'dan 0 yap!
    public float closeAnticipation = 1.5f;

    [Header("Glide Settings (If On)(Kayma Ayarlarý (Eðer Açýksa))")]
    public float startYOffset = -1500f; // Kayma açýksa menü ekranýn kaç piksel altýndan fýrlayacak?

    // Menünün Editor'de (Inspector'da) ayarladýðýn asýl (hedef) pozisyonu ve boyutu
    private Vector2 originalPosition;
    private Vector3 originalScale;

    private void Awake()
    {
        // Script ilk yüklendiðinde RectTransform bileþenini yakala
        rectTransform = GetComponent<RectTransform>();

        // Panelin durmasý gereken asýl hedef pozisyonunu ve boyutunu hafýzaya al
        originalPosition = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
    }

    // Obje SetActive(true) yapýldýðýnda otomatik olarak bu fonksiyon çalýþýr
    private void OnEnable()
    {
        // Kayarak gelecekse ekranýn altýna al, pop-up ise olduðu yerde kalsýn
        Vector2 offset = slideFromBottom ? new Vector2(0, startYOffset) : Vector2.zero;
        rectTransform.anchoredPosition = originalPosition + offset;

        // Kayarak geliyorsa yarý boyuttan baþla, pop-up ise 0'dan (görünmezden) baþla
        rectTransform.localScale = slideFromBottom ? originalScale * 0.5f : Vector3.zero;

        // 1 karelik parlama hatasýný engellemek için
        Canvas.ForceUpdateCanvases();

        // Önceki animasyonlarý durdur ve açýlma iþlemini baþlat
        StopAllCoroutines();
        StartCoroutine(BounceRoutine(true));
    }

    // Kapatma butonuna (X veya Geri) bu fonksiyonu baðlaman gerekiyor
    public void CloseMenu()
    {
        StopAllCoroutines();
        // Kapanma iþlemini baþlat
        StartCoroutine(BounceRoutine(false));
    }

    // Tüm animasyon matematiðinin döndüðü ana motor
    private IEnumerator BounceRoutine(bool isOpening)
    {
        // --- GECÝKME (DELAY) BEKLEMESÝ ---
        float delay = isOpening ? openDelay : closeDelay;
        if (delay > 0f)
        {
            // Pause menüsünde zaman durduðu için Realtime bekletiyoruz
            yield return new WaitForSecondsRealtime(delay);
        }

        float elapsed = 0f; // Geçen süreyi tutan sayaç
        float duration = isOpening ? openDuration : closeDuration;

        // --- BAÞLANGIÇ VE BÝTÝÞ POZÝSYONLARI ---
        Vector2 offset = slideFromBottom ? new Vector2(0, startYOffset) : Vector2.zero;
        Vector2 startPos = isOpening ? originalPosition + offset : originalPosition;
        Vector2 endPos = isOpening ? originalPosition : originalPosition + offset;

        // --- BAÞLANGIÇ VE BÝTÝÞ BOYUTLARI (SCALE) ---
        Vector3 minScale = slideFromBottom ? originalScale * 0.5f : Vector3.zero;
        Vector3 startScl = isOpening ? minScale : originalScale;
        Vector3 endScl = isOpening ? originalScale : minScale;

        // Belirlenen süre bitene kadar döngüyü çalýþtýr
        while (elapsed < duration)
        {
            // Zaman dursa bile animasyonun akmasý için unscaledDeltaTime
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration; // %0 ile %100 arasý ilerleme

            float scaleCurve;
            float posCurve;

            if (isOpening)
            {
                // AÇILMA MATEMATÝÐÝ (Ease Out Back): Hedefi aþýp geri döner
                float s = openOvershoot;
                float tempT = t - 1.0f;
                scaleCurve = tempT * tempT * ((s + 1) * tempT + s) + 1.0f;

                // Yukarý kayarken zýplamamasý için fren
                posCurve = 1f - Mathf.Pow(1f - t, 3f);
            }
            else
            {
                // KAPANMA MATEMATÝÐÝ (Ease In Back): Önce þiþer, sonra küçülür
                float s = closeAnticipation;
                scaleCurve = t * t * ((s + 1) * t - s);

                posCurve = t * t * t;
            }

            // LerpUnclamped ile efektleri uygula
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, posCurve);
            rectTransform.localScale = Vector3.LerpUnclamped(startScl, endScl, scaleCurve);

            yield return null; // Bir sonraki frame'e kadar bekle
        }

        // Animasyon bittiðinde asýl hedefe sabitle
        rectTransform.anchoredPosition = endPos;
        rectTransform.localScale = endScl;

        // Kapanma iþlemiyse objeyi tamamen gizle
        if (!isOpening) gameObject.SetActive(false);
    }
}