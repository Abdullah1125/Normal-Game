using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Handles extra hints with ad integration and global UI locking via Canvas Groups.
/// (Reklam entegrasyonu ve Canvas Group kilitleri ile ekstra ipuçlarını yönetir.)
/// </summary>
public class ExtraHintUI : MonoBehaviour
{
    [Header("Hint UI (İpucu Ekranı)")]
    public TextMeshProUGUI hintText;
    public Image hintImage;
    public GameObject mainHintPanel;

    [Header("Loading UI (Darlama Paneli)")]
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

    private void Start() => CheckLevelStatus();

    /// <summary>
    /// Checks if the hint for the current level is already unlocked.
    /// </summary>
    private void CheckLevelStatus()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            isHintUnlocked = (LevelManager.Instance.activeLevel.levelID == lastUnlockedLevelID);
        }
    }

    /// <summary>
    /// Handles the hint button click and locks the HUD.
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

        // İnternet yoksa 10 saniye darlama, varsa reklam yükle
        float timeout = (Application.internetReachability == NetworkReachability.NotReachable) ? 10.0f : 5.0f;
        StartCoroutine(SmartAdLoader(timeout));
    }

    /// <summary>
    /// Displays the hint content and pauses the game.
    /// </summary>
    public void ShowExtraHint()
    {
        LevelData currentLevelData = LevelManager.Instance.activeLevel;
        if (currentLevelData == null) return;

        TutorialTrigger.OnHintToggled?.Invoke(true);

        // Dil desteğine göre yazıyı çek
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            string[] allHints = LocalizationManager.Instance.currentData.extra_hints;
            if (allHints != null && currentLevelData.levelID < allHints.Length)
                hintText.text = allHints[currentLevelData.levelID];
        }

        // Görsel varsa yerleştir, yoksa metni genişlet
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

        HideWithFold(pauseButton);
        HideWithFold(extraHintButton);
    }

    /// <summary>
    /// Closes the hint panel and releases the UI lock after animation.
    /// </summary>
    public void CloseExtraHint()
    {
        // Panel butonu kilitlenir
        if (UIManager.Instance != null) UIManager.Instance.SetHintBlock(true);

        Time.timeScale = 1f;
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;

        ShowWithFold(pauseButton);
        ShowWithFold(extraHintButton);

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

        // Animasyon bitince kilitleri aç
        StartCoroutine(EnableUIAfterHint(0.475f));
    }

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
            // Reklam hazır mı kontrolü (Manager üzerinden)
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

    private void UnlockHint()
    {
        isHintUnlocked = true;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
            lastUnlockedLevelID = LevelManager.Instance.activeLevel.levelID;
    }

    private IEnumerator EnableUIAfterHint(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetHUDBlock(false);
            UIManager.Instance.SetHintBlock(false);
        }
    }

    private IEnumerator WaitAndShowTutorial()
    {
        yield return new WaitForSeconds(0.4f);
        TutorialTrigger.OnHintToggled?.Invoke(false);
    }

    private IEnumerator AnimateDots()
    {
        string baseText = "Reklam Yükleniyor";
        if (loadingText != null)
        {
            LocalizedText locText = loadingText.GetComponent<LocalizedText>();
            if (locText != null) { locText.UpdateText(); baseText = loadingText.text.Replace(".", ""); }
        }

        while (true)
        {
            for (int i = 0; i < 6; i++)
            {
                if (loadingText != null) loadingText.text = baseText + new string('.', i);
                yield return new WaitForSecondsRealtime(0.4f);
            }
        }
    }

    private void HideWithFold(GameObject obj)
    {
        if (obj == null) return;
        var anim = obj.GetComponent<ButtonFoldAnimator>();
        if (anim != null && obj.activeInHierarchy) anim.HideButton(); else obj.SetActive(false);
    }

    private void ShowWithFold(GameObject obj)
    {
        if (obj == null) return;
        var anim = obj.GetComponent<ButtonFoldAnimator>();
        if (anim != null) anim.ShowButton(); else obj.SetActive(true);
    }
}