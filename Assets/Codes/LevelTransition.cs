using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance;

    [Header("Cinematic Doors(Sinematik Kapılar)")]
    public RectTransform topPanel;    // Üst kapı paneli
    public RectTransform bottomPanel; // Alt kapı paneli
    public float doorSpeed = 0.5f;

    [Header("Settings(Ayarlar)")]
    public bool openDoorsOnStart = true; // Sahne açıldığında kapılar otomatik açılsın mı?
    public TextMeshProUGUI levelText;

    // Sahne geçişini kontrol etmek için statik değişken
    public static bool isComingFromDoorTransition = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        float h = GetCanvasHeight();
        float w = GetCanvasWidth() + 500f; // Sadece genişlikte güvenlik payı bırakıyoruz

        if (topPanel != null && bottomPanel != null)
        {
            // DÜZELTME: Yükseklikleri tam ekranın yarısı yapıyoruz (+50 sildik).
            // Bu sayede iç içe geçme ve resimlerin aşağı/yukarı kayma sorunu çözülür.
            topPanel.sizeDelta = new Vector2(w, h / 2f);
            bottomPanel.sizeDelta = new Vector2(w, h / 2f);

            // Eğer yeni sahneye kapı geçişiyle geldiysek veya başlangıçta kapıların açılmasını istiyorsak
            if (isComingFromDoorTransition || openDoorsOnStart)
            {
                // Kapıları EKRANDA ve KAPALI başlat
                topPanel.gameObject.SetActive(true);
                bottomPanel.gameObject.SetActive(true);

                // Tam ortada (y=0 çizgisinde) jilet gibi birleşecekleri koordinatlar
                topPanel.anchoredPosition = new Vector2(0, h / 4f);
                bottomPanel.anchoredPosition = new Vector2(0, -h / 4f);
            }
            else
            {
                // Normal başlangıçsa kapılar zaten görünmez kalacak (Editor'de kapattığın gibi)
                topPanel.anchoredPosition = new Vector2(0, h);
                bottomPanel.anchoredPosition = new Vector2(0, -h);

                topPanel.gameObject.SetActive(false);
                bottomPanel.gameObject.SetActive(false);
            }
        }

        if (levelText != null) levelText.alpha = 0f;
    }

    IEnumerator Start()
    {
        // Eğer geçişten geliyorsak, sahne yüklendiğinde kapıları otomatik aç
        if (isComingFromDoorTransition || openDoorsOnStart)
        {
            isComingFromDoorTransition = false;
            yield return new WaitForSeconds(0.2f); // Sahnede her şeyin yüklenmesini çok kısa bekle
            yield return OpenDoorsRoutine();
        }
    }

    // SAHNELER ARASI GEÇİŞ İÇİN
    public void FadeOut(System.Action onComplete = null)
    {
        StartCoroutine(CloseDoorsRoutine(() =>
        {
            isComingFromDoorTransition = true;
            onComplete?.Invoke();
        }));
    }

    // AYNI SAHNE İÇİNDE BÖLÜM DEĞİŞTİRMEK İÇİN 
    public void DoTransition(System.Action middleAction)
    {
        StartCoroutine(TransitionRoutine("", middleAction));
    }

    public void DoTransition(string message, System.Action middleAction)
    {
        StartCoroutine(TransitionRoutine(message, middleAction));
    }

    // ANA ANİMASYON RUTİNLERİ 

    private IEnumerator TransitionRoutine(string message, System.Action middleAction)
    {
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        if (levelText != null) levelText.text = message;

        // 1. Kapıları Kapat
        yield return CloseDoorsRoutine(null);

        // 2. Kapılar kapalıyken arka planda yapılacakları yap
        if (levelText != null) levelText.alpha = 1f;
        middleAction?.Invoke();
        yield return new WaitForSeconds(0.4f); // Siyah ekranda kalma süresi

        // 3. Yazıyı gizle ve Kapıları Aç
        if (levelText != null) levelText.alpha = 0f;
        yield return OpenDoorsRoutine();

        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;
    }

    private IEnumerator CloseDoorsRoutine(System.Action onComplete)
    {
        float h = GetCanvasHeight();

        // Animasyon başlamadan önce objeleri AKTİF (görünür) yap
        if (topPanel != null) topPanel.gameObject.SetActive(true);
        if (bottomPanel != null) bottomPanel.gameObject.SetActive(true);

        // Açık (h, -h) pozisyonundan -> Kapalı (h/4, -h/4) pozisyonuna hareket
        yield return MoveDoors(new Vector2(0, h), new Vector2(0, h / 4f),
                               new Vector2(0, -h), new Vector2(0, -h / 4f));
        onComplete?.Invoke();
    }

    private IEnumerator OpenDoorsRoutine()
    {
        float h = GetCanvasHeight();

        // Kapalı (h/4, -h/4) pozisyonundan -> Açık (h, -h) pozisyonuna hareket
        yield return MoveDoors(new Vector2(0, h / 4f), new Vector2(0, h),
                               new Vector2(0, -h / 4f), new Vector2(0, -h));

        // Kapılar tamamen açıldıktan sonra GÖRÜNMEZ YAP (Tıklamaları engellemesin)
        if (topPanel != null) topPanel.gameObject.SetActive(false);
        if (bottomPanel != null) bottomPanel.gameObject.SetActive(false);
    }

    private IEnumerator MoveDoors(Vector2 tStart, Vector2 tEnd, Vector2 bStart, Vector2 bEnd)
    {
        float elapsed = 0f;

        while (elapsed < doorSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / doorSpeed;
            t = t * t * (3f - 2f * t); // SmoothStep: Animasyonun başı ve sonu yumuşak olur

            if (topPanel != null) topPanel.anchoredPosition = Vector2.Lerp(tStart, tEnd, t);
            if (bottomPanel != null) bottomPanel.anchoredPosition = Vector2.Lerp(bStart, bEnd, t);

            yield return null;
        }

        if (topPanel != null) topPanel.anchoredPosition = tEnd;
        if (bottomPanel != null) bottomPanel.anchoredPosition = bEnd;
    }

    // Yardımcı Matematik Fonksiyonları
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
}