using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Reklam entegrasyonu ve Canvas Group kilitleri ile ekstra ipuçlarını yönetir.
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
    /// Bileşen referanslarını oyun başlamadan önce önbelleğe alır (Caching).
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

    private void ResetHintStatus()
    {
        isHintUnlocked = false;
        lastUnlockedLevelID = -1;
    }

    /// <summary>
    /// İpucu butonuna tıklandığında çalışır ve arayüzü kilitler.
    /// </summary>
    public void OnHintButtonClick()
    {
        // HUD butonlarını dondur (Spam koruması)
        if (UIManager.Instance != null) UIManager.Instance.SetHUDBlock(true);

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
    /// İpucu panelini gösterir ve oyun zamanını durdurur.
    /// </summary>
    public void ShowExtraHint()
    {
        LevelData currentLevelData = LevelManager.Instance.activeLevel;
        if (currentLevelData == null) return;

        TutorialTrigger.OnHintToggled?.Invoke(true);

        // İpucu metnini yerelleştirme verilerinden çek
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            string[] allHints = LocalizationManager.Instance.currentData.extra_hints;
            if (allHints != null && currentLevelData.levelID < allHints.Length)
                hintText.text = allHints[currentLevelData.levelID];
        }

        // Görselin varlığına göre UI düzenini ayarla
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
    /// İpucu panelini kapatır ve arayüz kilitlerini serbest bırakır.
    /// </summary>
    public void CloseExtraHint()
    {
        // İpucu paneli geçiş süresince diğer tıklamaları engelle
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
    /// Reklam yükleme sürecini yönetir.
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
    /// Mevcut seviyenin ipucunu açılmış olarak kaydeder.
    /// </summary>
    private void UnlockHint()
    {
        isHintUnlocked = true;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
            lastUnlockedLevelID = LevelManager.Instance.activeLevel.levelID;
    }

    /// <summary>
    /// Belirtilen gecikme sonrasında arayüz kilitlerini kaldırır.
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
    /// Eğitici içeriği tetiklemek için kısa bir süre bekler.
    /// </summary>
    private IEnumerator WaitAndShowTutorial()
    {
        yield return new WaitForSeconds(0.4f);
        TutorialTrigger.OnHintToggled?.Invoke(false);
    }

    /// <summary>
    /// Yükleme ekranındaki hareketli nokta animasyonunu oynatır.
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

    /// <summary>
    /// Hedef butonu katlanma animasyonu ile gizler.
    /// </summary>
    private void HideWithFold(GameObject obj, ButtonFoldAnimator anim)
    {
        if (obj == null) return;
        if (anim != null && obj.activeInHierarchy) anim.HideButton(); else obj.SetActive(false);
    }

    /// <summary>
    /// Hedef butonu katlanma animasyonu ile görünür hale getirir.
    /// </summary>
    private void ShowWithFold(GameObject obj, ButtonFoldAnimator anim)
    {
        if (obj == null) return;
        if (anim != null) anim.ShowButton(); else obj.SetActive(true);
    }
}