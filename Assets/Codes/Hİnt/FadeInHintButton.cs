using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInHintButton : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDelay = 20.0f; // Bekleme süresi
    public float fadeSpeed = 0.8f;  // Belirme hýzý

    private float timeSpentInLevel = 0f; // Levelde geçirilen zaman
    private bool isFadedIn = false;      // Buton tamamen belirdi mi?
    private int currentLevelID = -1;     // Hangi leveldeyiz hafýzasý

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        // LevelManager'dan gelen sinyali dinle
        LevelManager.OnLevelStarted += CheckLevelChange;

        // Obje aktif olduðunda (Geri tuþuna basýldýðýnda) hemen durumu kontrol et
        CheckLevelChange();
    }

    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= CheckLevelChange;
    }

    // Bu fonksiyon hem level deðiþince hem de Geri tuþuna basýnca çalýþýr
    private void CheckLevelChange()
    {
        int newLevelID = -1;
        if (LevelManager.Instance != null && LevelManager.Instance.activeLevel != null)
        {
            newLevelID = LevelManager.Instance.activeLevel.levelID;
        }

        // DURUM 1: Eðer YENÝ BÝR BÖLÜME geçildiyse her þeyi sýfýrla!
        if (newLevelID != currentLevelID)
        {
            currentLevelID = newLevelID;
            isFadedIn = false;
            timeSpentInLevel = 0f;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        // DURUM 2: Ayný leveldeysek (Öldüysek veya Ýpucu panelini kapattýysak)
        else if (isFadedIn)
        {
            // Eðer buton zaten belirdiyse ZORLA GÖRÜNÜR YAP, sayacý bekleme!
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        // Buton zaten görünürse Update'i boþuna yorma
        if (isFadedIn) return;

        // Oyun akarken süreyi say
        timeSpentInLevel += Time.deltaTime;

        if (timeSpentInLevel >= fadeDelay)
        {
            if (canvasGroup.alpha < 1f)
            {
                canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                isFadedIn = true; // Görev bitti, buton ekrana kazýndý!
            }
        }
    }
}