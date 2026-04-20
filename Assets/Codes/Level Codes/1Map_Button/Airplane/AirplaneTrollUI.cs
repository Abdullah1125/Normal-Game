using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AirplaneUI : MonoBehaviour
{
    public static AirplaneUI Instance;

    private CanvasGroup canvasGroup;

    [Header("UI Speed Settings (UI Hız Ayarları)")]
    public float fadeInSpeed = 5.0f;
    public float fadeOutSpeed = 3.0f;

    [Header("Pause Menü Bekleme Ayarı")]
    public float delayAfterPause = 1.0f; 

    private bool isVisible = false;
    private bool isRecoveringFromPause = false; // Pause'dan uyanma kalkanı
    private float delayTimer = 0f;

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        HideImmediately();
    }

    private void Update()
    {
        // 1. ZAMAN DURDUĞUNDA (Pause açılınca)
        if (Time.timeScale == 0f)
        {
            if (isVisible)
            {
                isVisible = false;             // Kapanmayı başlat
                isRecoveringFromPause = true;  // Kalkanı aç (bekleme sürecine gir)
                delayTimer = 0f;               // Sayacı sıfırla
            }
        }
        // ZAMAN AKTIĞINDA (Resume tuşuna basılınca)
        else if (Time.timeScale > 0f && isRecoveringFromPause)
        {
            delayTimer += Time.unscaledDeltaTime; // Gerçek zamanla sayacı artır

            // Belirlediğimiz süre dolduysa
            if (delayTimer >= delayAfterPause)
            {
                isRecoveringFromPause = false; // Kalkanı indir
                isVisible = true;              // Uçağı yavaşça aç
            }
        }

        // --- GÖRSELLİK (ALFA) AYARLARI ---
        if (isVisible && canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.unscaledDeltaTime * fadeInSpeed;
        }
        else if (!isVisible && canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime * fadeOutSpeed;
        }

        // Objeyi kapatmak yerine sadece etkileşimi kesiyoruz
        bool isAlphaVisible = canvasGroup.alpha > 0f;
        canvasGroup.interactable = isAlphaVisible;
        canvasGroup.blocksRaycasts = isAlphaVisible;
    }

    public void ShowPanel()
    {
        //Eğer pause'dan çıkış bekliyorsak veya zaman durmuşsa gelen emirleri REDDET!
        if (Time.timeScale == 0f || isRecoveringFromPause) return;

        isVisible = true;
    }

    public void HidePanel()
    {
        isVisible = false;
        isRecoveringFromPause = false; // Dışarıdan kapatılırsa kalkanı da indir
    }

    public void HideImmediately()
    {
        isVisible = false;
        isRecoveringFromPause = false;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}