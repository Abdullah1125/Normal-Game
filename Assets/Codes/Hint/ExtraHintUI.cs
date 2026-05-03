using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Manages extra hints with ad integration, UI locks, and session-based memory.
/// (Reklam entegrasyonu, arayüz kilitleri ve oturum tabanlı hafıza ile ekstra ipuçlarını yönetir.)
/// </summary>
public class ExtraHintUI : MonoBehaviour
{
    [Header("Hint UI (İpucu Ekranı)")]
    public TextMeshProUGUI hintText;
    public Image hintImage;
    public GameObject mainHintPanel;

    [Header("Loading UI (Yükleme Paneli)")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;

    [Header("Buttons (Butonlar)")]
    public GameObject pauseButton;
    public GameObject extraHintButton;

    [Header("Settings (Ayarlar)")]
    public RectTransform textRect;
    public float textWidthWithoutImage = 800f;
    public float textWidthWithImage = 450f;
    public MenuBounceAnimator hintAnimator;

    private static bool isHintUnlocked = false;
    public static int lastUnlockedLevelID = -1;
    private Coroutine _dotsCoroutine;

    // Önbelleğe alınmış bileşen referansları
    private ButtonFoldAnimator pauseButtonAnim;
    private ButtonFoldAnimator extraHintButtonAnim;

    /// <summary>
    /// Caches component references before the game starts.
    /// (Bileşen referanslarını oyun başlamadan önce önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        if (pauseButton != null)
            pauseButtonAnim = pauseButton.GetComponent<ButtonFoldAnimator>();

        if (extraHintButton != null)
            extraHintButtonAnim = extraHintButton.GetComponent<ButtonFoldAnimator>();
    }

    private void OnEnable()
    {
        LevelManager.OnLevelStarted += ResetHintStatus;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ResetHintStatus;
    }

    /// <summary>
    /// Resets the hint status only if a DIFFERENT level is loaded.
    /// (Sadece FARKLI bir bölüm yüklendiğinde ipucu durumunu sıfırlar. Ölünce açık kalmasını sağlar.)
    /// </summary>
    private void ResetHintStatus()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            if (LevelManager.Instance.activeLevel.levelID != lastUnlockedLevelID)
            {
                isHintUnlocked = false;
                lastUnlockedLevelID = -1;
            }
        }
        else
        {
            isHintUnlocked = false;
            lastUnlockedLevelID = -1;
        }
    }

    /// <summary>
    /// Locks the UI, instantly pauses the game, and checks unlock status.
    /// (Arayüzü kilitler, oyunu anında durdurur ve kilit durumunu kontrol eder.)
    /// </summary>
    public void OnHintButtonClick()
    {
        // HUD butonlarını dondur (Spam koruması)
        if (UIManager.Instance != null) UIManager.Instance.SetHUDBlock(true);

        // JİLET: Reklamın yüklenmesini beklemeden oyun zamanını anında durdur. (Karakter düşmesin)
        Time.timeScale = 0f;
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;

        if (isHintUnlocked)
        {
            ShowExtraHint();
            return;
        }

        // İnternet bağlantısına göre zaman aşımı süresini belirle
        float timeout = (Application.internetReachability == NetworkReachability.NotReachable) ? 10.0f : 5.0f;
        StartCoroutine(SmartAdLoader(timeout));
    }

    /// <summary>
    /// Shows the hint panel.
    /// (İpucu panelini gösterir.)
    /// </summary>
    public void ShowExtraHint()
    {
        LevelData currentLevelData = LevelManager.Instance.activeLevel;
        if (currentLevelData == null) return;

        TutorialTrigger.OnHintToggled?.Invoke(true);

        if (LocalizationManager.Instance != null)
        {
            string translatedExtraHint = LocalizationManager.Instance.GetLevelText(currentLevelData.levelID, "extra");
            if (!string.IsNullOrEmpty(translatedExtraHint))
            {
                hintText.text = translatedExtraHint;
            }
        }

        if (currentLevelData.hintVisualMap != null)
        {
            hintImage.sprite = currentLevelData.hintVisualMap;
            hintImage.gameObject.SetActive(true);
            textRect.sizeDelta = new Vector2(textWidthWithImage, textRect.sizeDelta.y);
        }
        else
        {
            hintImage.gameObject.SetActive(false);
            textRect.sizeDelta = new Vector2(textWidthWithoutImage, textRect.sizeDelta.y);
        }

        Time.timeScale = 0f;
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
        mainHintPanel.SetActive(true);

        HideWithFold(pauseButton, pauseButtonAnim);
        HideWithFold(extraHintButton, extraHintButtonAnim);
    }

    /// <summary>
    /// Closes the hint panel and releases UI locks.
    /// (İpucu panelini kapatır ve arayüz kilitlerini serbest bırakır.)
    /// </summary>
    public void CloseExtraHint()
    {
        if (UIManager.Instance != null) UIManager.Instance.SetHintBlock(true);

        Time.timeScale = 1f;
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;

        ShowWithFold(pauseButton, pauseButtonAnim);
        ShowWithFold(extraHintButton, extraHintButtonAnim);

        if (hintAnimator != null)
        {
            hintAnimator.CloseMenu();
            StartCoroutine(WaitAndShowTutorial());
        }
        else
        {
            mainHintPanel.SetActive(false);
            TutorialTrigger.OnHintToggled?.Invoke(false);
        }

        StartCoroutine(EnableUIAfterHint(0.475f));
    }

    /// <summary>
    /// Manages the ad loading process using unscaled time.
    /// (Etkilenmeyen zaman kullanarak reklam yükleme sürecini yönetir.)
    /// </summary>
    private IEnumerator SmartAdLoader(float timeout)
    {
        PauseManager.isAdLoading = true;
        if (loadingPanel != null) loadingPanel.SetActive(true);
        _dotsCoroutine = StartCoroutine(AnimateDots());

        float timer = 0f;
        bool adOpened = false;

        while (timer < timeout)
        {
            timer += Time.unscaledDeltaTime;

            if (AdMobRewardedManager.Instance != null && AdMobRewardedManager.Instance.IsAdReady())
            {
                adOpened = true;
                if (_dotsCoroutine != null) StopCoroutine(_dotsCoroutine);
                if (loadingPanel != null) loadingPanel.SetActive(false);
                PauseManager.isAdLoading = false;

                AdMobRewardedManager.Instance.ShowRewardedAd(() => {
                    UnlockHint();
                    ShowExtraHint();
                });
                break;
            }
            yield return null;
        }

        if (!adOpened)
        {
            if (_dotsCoroutine != null) StopCoroutine(_dotsCoroutine);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            PauseManager.isAdLoading = false;
            UnlockHint();
            ShowExtraHint();
        }
    }

    /// <summary>
    /// Saves the unlocked status of the current level to memory for this session.
    /// (Mevcut seviyenin açılma durumunu bu oturum için hafızaya kaydeder.)
    /// </summary>
    private void UnlockHint()
    {
        isHintUnlocked = true;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            lastUnlockedLevelID = LevelManager.Instance.activeLevel.levelID;
        }
    }

    /// <summary>
    /// Clears the hint memory completely. Call this when returning to the Main Menu.
    /// (İpucu hafızasını tamamen siler. Ana Menüye dönüldüğünde bunu çağırın.)
    /// </summary>
    public static void ClearHintMemory()
    {
        lastUnlockedLevelID = -1;
        isHintUnlocked = false;
    }

    /// <summary>
    /// Removes UI locks after a specified delay.
    /// (Belirtilen gecikme sonrasında arayüz kilitlerini kaldırır.)
    /// </summary>
    private IEnumerator EnableUIAfterHint(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDBlock(false);
            UIManager.Instance.SetHintBlock(false);
        }
    }

    /// <summary>
    /// Waits briefly before triggering tutorial content.
    /// (Eğitici içeriği tetiklemek için kısa bir süre bekler.)
    /// </summary>
    private IEnumerator WaitAndShowTutorial()
    {
        yield return new WaitForSecondsRealtime(0.4f);
        TutorialTrigger.OnHintToggled?.Invoke(false);
    }

    /// <summary>
    /// Animates the loading dots on the ad panel.
    /// (Reklam panelindeki yükleme noktalarını canlandırır.)
    /// </summary>
    private IEnumerator AnimateDots()
    {
        string baseText = (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
            ? LocalizationManager.Instance.currentData.ads_panel
            : "Loading Ad";

        while (true)
        {
            for (int i = 0; i < 6; i++)
            {
                if (loadingText != null) loadingText.text = baseText + new string('.', i);
                yield return new WaitForSecondsRealtime(0.4f);
            }
        }
    }

    private void HideWithFold(GameObject obj, ButtonFoldAnimator anim)
    {
        if (obj == null) return;
        if (anim != null && obj.activeInHierarchy) anim.HideButton(); else obj.SetActive(false);
    }

    private void ShowWithFold(GameObject obj, ButtonFoldAnimator anim)
    {
        if (obj == null) return;
        if (anim != null) anim.ShowButton(); else obj.SetActive(true);
    }
}