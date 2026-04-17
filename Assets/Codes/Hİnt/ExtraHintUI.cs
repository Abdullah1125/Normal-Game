using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExtraHintUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI hintText;
    public Image hintImage;

    [Header("Buttons to Hide(Gizlenecek Butonlar)")]
    public GameObject pauseButton;
    public GameObject extraHintButton;

    [Header("Text Width (X) and Margin Settings(Yazý Geniþlik (X) ve Marj Ayarlarý)")]
    public RectTransform textRect; 
    public float leftMargin = 50f; 
    public float textWidthWithoutImage = 800f; 
    public float textWidthWithImage = 450f;    

 
    public void ShowExtraHint()
    {
        LevelData currentLevelData = LevelManager.Instance.activeLevel;
        if (currentLevelData == null) return;

        // 1. YAZIYI ÇEK VE BAS 
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.currentData != null)
        {
            int levelIndex = currentLevelData.levelID;
            string[] allHints = LocalizationManager.Instance.currentData.extra_hints;
            if (allHints != null && levelIndex < allHints.Length) hintText.text = allHints[levelIndex];
        }

        //  2. DÝNAMÝK YAZI SABÝTLEME VE DARALTMA 
        if (textRect != null)
        {
            
            textRect.pivot = new Vector2(0f, 0.5f); 
            textRect.anchorMax = new Vector2(0f, 0.5f);

            textRect.anchoredPosition = new Vector2(leftMargin, textRect.anchoredPosition.y);
        }

        // 3. FOTOÐRAF VE YAZI GENÝÞLÝÐÝ 
        if (currentLevelData.hintVisualMap != null)
        {
            hintImage.sprite = currentLevelData.hintVisualMap;
            hintImage.gameObject.SetActive(true);

            if (textRect != null) textRect.sizeDelta = new Vector2(textWidthWithImage, textRect.sizeDelta.y);
        }
        else
        {
            hintImage.gameObject.SetActive(false);

            if (textRect != null) textRect.sizeDelta = new Vector2(textWidthWithoutImage, textRect.sizeDelta.y);
        }

        // 4. BUTONLARI GÝZLE VE ZAMANI DONDUR
        if (pauseButton != null) pauseButton.SetActive(false);
        if (extraHintButton != null) extraHintButton.SetActive(false);
        Time.timeScale = 0f;

        gameObject.SetActive(true);
    }

 
    public void CloseExtraHint()
    {
        if (pauseButton != null) pauseButton.SetActive(true);
        if (extraHintButton != null) extraHintButton.SetActive(true);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}