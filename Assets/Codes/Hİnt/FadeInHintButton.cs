using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadeInHintButton : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    public float fadeDelay = 20.0f; // Bekleme süresi
    public float fadeSpeed = 0.8f;  // Belirme hýzý

    private float startTime;
    private bool isCounting = false;
    private bool hasBeenClickedInThisLevel = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        // Butona týklandýðýnda OnButtonClick fonksiyonunu çaðýr
        GetComponent<Button>().onClick.AddListener(OnButtonClick);
    }

    private void OnEnable()
    {
        // LevelManager'dan gelen "Yeni Level/Reset" sinyalini dinle
        LevelManager.OnLevelStarted += ResetForNewLevel;
        ResetForNewLevel();
    }

    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ResetForNewLevel;
    }

    // SADECE LevelManager tetiklediðinde (Level geçince veya ölünce) çalýþýr
    private void ResetForNewLevel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        startTime = Time.time;
        isCounting = true;
        hasBeenClickedInThisLevel = false; // Týklanma bilgisini sýfýrla
    }

    // Butona (Ýpucuya) basýldýðýnda çalýþýr
    public void OnButtonClick()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        isCounting = false; // Artýk sayma, bu level'da iþi bitti
        hasBeenClickedInThisLevel = true;
    }

    // Geri tuþuna basýldýðýnda (Panel kapanýnca) butonun geri gelmesi için

    public void ShowButtonAgain()
    {
        // Eðer bu level'da zaten týklanmýþsa ve süre dolmuþsa geri getir
        if (hasBeenClickedInThisLevel)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private void Update()
    {
        if (!isCounting) return;

        if (Time.time - startTime >= fadeDelay)
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
                isCounting = false;
            }
        }
    }
}