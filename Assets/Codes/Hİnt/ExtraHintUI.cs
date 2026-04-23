using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class ExtraHintUI : MonoBehaviour
{
    [Header("Hint UI (İpucu Ekranı)")]
    public TextMeshProUGUI hintText;
    public Image hintImage;
    public GameObject mainHintPanel;

    [Header("Loading UI (Darlama Paneli)")]
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;

    [Header("Buttons to Hide (Gizlenecek Butonlar)")]
    public GameObject pauseButton;
    public GameObject extraHintButton;

    [Header("Settings (Ayarlar)")]
    public RectTransform textRect;
    public float leftMargin = 50f;
    public float textWidthWithoutImage = 800f;
    public float textWidthWithImage = 450f;
    public MenuBounceAnimator hintAnimator;

    private static bool isHintUnlocked = false;
    public static int lastUnlockedLevelID = -1;

    private Coroutine _dotsCoroutine;

    void Start()
    {
        CheckLevelStatus();
    }

    private void CheckLevelStatus()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            if (LevelManager.Instance.activeLevel.levelID == lastUnlockedLevelID)
            {
                isHintUnlocked = true;
            }
            else
            {
                isHintUnlocked = false;
            }
        }
    }

    /// <summary>
    /// Handles the hint button click and locks the button to prevent spam.
    /// (İpucu butonuna tıklanmasını işler ve spam'i önlemek için butonu kilitler.)
    /// </summary>
    public void OnHintButtonClick()
    {
        // 1. ŞALTERİ İNDİR: Oyuncu bir kere bastıysa butonu kilitliyoruz!
        if (extraHintButton != null)
        {
            extraHintButton.GetComponent<Button>().interactable = false;
        }

        if (isHintUnlocked)
        {
            ShowExtraHint();
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
            StartCoroutine(SmartAdLoader(10.0f));
            return;
        }

        if (AdMobRewardedManager.Instance != null)
        {
            if (AdMobRewardedManager.Instance.IsAdReady())
            {
                AdMobRewardedManager.Instance.ShowRewardedAd(() => {
                    UnlockHint();
                    ShowExtraHint();
                });
            }
            else
            {
                if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
                AdMobRewardedManager.Instance.LoadRewardedAd();
                StartCoroutine(SmartAdLoader(5.0f));
            }
        }
        else
        {
            StartCoroutine(SmartAdLoader(10.0f));
        }
    }

    private void UnlockHint()
    {
        isHintUnlocked = true;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            lastUnlockedLevelID = LevelManager.Instance.activeLevel.levelID;
        }
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

    private void ShowExtraHint()
    {
        LevelData currentLevelData = LevelManager.Instance.activeLevel;
        if (currentLevelData == null) return;

        TutorialTrigger.OnHintToggled?.Invoke(true);

        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            string[] allHints = LocalizationManager.Instance.currentData.extra_hints;
            if (allHints != null && currentLevelData.levelID < allHints.Length)
                hintText.text = allHints[currentLevelData.levelID];
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

        HideWithFold(pauseButton);
        HideWithFold(extraHintButton);
    }

    /// <summary>
    /// Closes the hint panel and unlocks the button.
    /// (İpucu panelini kapatır ve butonun kilidini açar.)
    /// </summary>
    public void CloseExtraHint()
    {
        Time.timeScale = 1f;
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;

        // 2. DÜZELTME: Butonları SetActive(true) yerine animasyonlu açıyoruz.
        ShowWithFold(pauseButton);
        ShowWithFold(extraHintButton);

        // 3. ŞALTERİ KALDIR: İpucu paneli kapandığına göre butonun kilidini açıyoruz.
        if (extraHintButton != null)
        {
            extraHintButton.GetComponent<Button>().interactable = true;
        }

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
    }

    private IEnumerator WaitAndShowTutorial()
    {
        yield return new WaitForSeconds(0.4f);
        TutorialTrigger.OnHintToggled?.Invoke(false);
    }

    private void OnEnable() { LevelManager.OnLevelStarted += ResetHintLock; }
    private void OnDisable() { LevelManager.OnLevelStarted -= ResetHintLock; }

    private void ResetHintLock()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            if (LevelManager.Instance.activeLevel.levelID != lastUnlockedLevelID)
            {
                isHintUnlocked = false;
                lastUnlockedLevelID = -1;
                Debug.Log("Yeni Level: Hafıza silindi.");
            }
        }
    }

    private void HideWithFold(GameObject buttonObj)
    {
        if (buttonObj == null) return;
        ButtonFoldAnimator foldAnim = buttonObj.GetComponent<ButtonFoldAnimator>();
        if (foldAnim != null && buttonObj.activeInHierarchy) foldAnim.HideButton();
        else buttonObj.SetActive(false);
    }

    // YENİ EKLENDİ: Animasyonlu açma motoru
    private void ShowWithFold(GameObject buttonObj)
    {
        if (buttonObj == null) return;
        ButtonFoldAnimator foldAnim = buttonObj.GetComponent<ButtonFoldAnimator>();
        if (foldAnim != null) foldAnim.ShowButton();
        else buttonObj.SetActive(true);
    }

    private IEnumerator AnimateDots()
    {
        string baseText = "Reklam Yükleniyor";

        if (loadingText != null)
        {
            LocalizedText locText = loadingText.GetComponent<LocalizedText>();
            if (locText != null)
            {
                locText.UpdateText();
                baseText = loadingText.text;
                baseText = baseText.Replace(".", "");
            }
        }

        TutorialTrigger.OnHintToggled?.Invoke(true);

        while (true)
        {
            for (int i = 0; i < 6; i++)
            {
                if (loadingText != null)
                {
                    loadingText.text = baseText + new string('.', i);
                }

                yield return new WaitForSecondsRealtime(0.4f);
            }
        }
    }
}