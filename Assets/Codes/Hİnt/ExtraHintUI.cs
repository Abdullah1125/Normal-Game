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

    [Header("Buttons to Hide")]
    public GameObject pauseButton;
    public GameObject extraHintButton;

    [Header("Settings")]
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
        // Sahne her açıldığında kontrol et: "Ben bu levelin reklamını az önce izledim mi?"
        CheckLevelStatus();
    }

    private void CheckLevelStatus()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            // Eğer şu anki level ID'si, hafızadaki ile aynıysa kilidi açık tut (ÖLÜNCE BURASI ÇALIŞIR)
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

    public void OnHintButtonClick()
    {
        if (isHintUnlocked)
        {
            ShowExtraHint();
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (PlayerController.Instance != null) PlayerController.Instance.canMove = false;
            StartCoroutine(SmartAdLoader(3.0f));
            return;
        }

        if (AdMobRewardedManager.Instance != null)
        {
            //  Reklam hazırsa şak diye aç, değilse5  saniye süre ver
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
            StartCoroutine(SmartAdLoader(5.0f));
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
        if (loadingPanel != null) loadingPanel.SetActive(true);
        _dotsCoroutine = StartCoroutine(AnimateDots());

        float timer = 0f;
        bool adOpened = false;

        // Belirlenen süre (5sn) boyunca her frame reklamı kontrol et
        while (timer < timeout)
        {
            timer += Time.unscaledDeltaTime;

            if (AdMobRewardedManager.Instance != null && AdMobRewardedManager.Instance.IsAdReady())
            {
                adOpened = true;

                if (_dotsCoroutine != null) StopCoroutine(_dotsCoroutine);
                if (loadingPanel != null) loadingPanel.SetActive(false);

                AdMobRewardedManager.Instance.ShowRewardedAd(() => {
                    UnlockHint();
                    ShowExtraHint();
                });
                break;
            }
            yield return null;
        }

        // 5 saniye bittiğinde hala reklam yoksa bedava ipucunu ver
        if (!adOpened)
        {
            if (_dotsCoroutine != null) StopCoroutine(_dotsCoroutine);
            if (loadingPanel != null) loadingPanel.SetActive(false);

            UnlockHint();
            ShowExtraHint();
        }
    }
    private void ShowExtraHint()
    {
        LevelData currentLevelData = LevelManager.Instance.activeLevel;
        if (currentLevelData == null) return;

        TutorialTrigger.OnHintToggled?.Invoke(true);

        // Yazıları çek
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            string[] allHints = LocalizationManager.Instance.currentData.extra_hints;
            if (allHints != null && currentLevelData.levelID < allHints.Length)
                hintText.text = allHints[currentLevelData.levelID];
        }

        // Görseli ayarla
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

   
    public void CloseExtraHint()
    {
        Time.timeScale = 1f;
        if (PlayerController.Instance != null) PlayerController.Instance.canMove = true;
        if (pauseButton != null) pauseButton.SetActive(true);
        if (extraHintButton != null) extraHintButton.SetActive(true);

        if (hintAnimator != null)
        {
            hintAnimator.CloseMenu();
            // Animasyon bitene kadar bekle, sonra hayaleti göster
            StartCoroutine(WaitAndShowTutorial());
        }
        else
        {
            mainHintPanel.SetActive(false);
            // Animasyon yoksa anında hayaleti geri çağır
            TutorialTrigger.OnHintToggled?.Invoke(false);
        }
    }

  
    private IEnumerator WaitAndShowTutorial()
    {
        //  Kapanma animasyonu süresi 
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
                lastUnlockedLevelID = -1; // Farklı levele geçince hafızayı sil
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

    private IEnumerator AnimateDots()
    {
        string baseText = "Reklam Yükleniyor"; // Varsayılan yedek metin

        //Objede LocalizedText varsa çeviriyi ona yaptırıyoruz!
        if (loadingText != null)
        {
            LocalizedText locText = loadingText.GetComponent<LocalizedText>();
            if (locText != null)
            {
                // Çeviriyi anında yapmasını emrediyoruz
                locText.UpdateText();

                // Çevrilmiş tertemiz yazıyı (Örn: "Ad Loading") alıyoruz
                baseText = loadingText.text;

                // Ne olur ne olmaz, önceden kalma noktalar varsa temizliyoruz
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