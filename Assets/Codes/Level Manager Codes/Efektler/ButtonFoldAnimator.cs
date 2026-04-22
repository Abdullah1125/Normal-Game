using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ButtonFoldAnimator : MonoBehaviour
{
    [Header("Fold Settings (Katlanma Ayarlarý)")]
    [Tooltip("Higher value means faster animation. (Deðer arttýkça daha hýzlý katlanýr/açýlýr.)")]
    public float foldSpeed = 20f;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isFolding = false;

    /// <summary>
    /// Initializes components and saves the original scale.
    /// (Bileþenleri baþlatýr ve orijinal boyutu kaydeder.)
    /// </summary>
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        targetScale = originalScale;
    }

    /// <summary>
    /// Resets the animation state when the object is enabled.
    /// (Obje aktif edildiðinde animasyon durumunu sýfýrlar.)
    /// </summary>
    private void OnEnable()
    {
        targetScale = originalScale;
        isFolding = false;
    }

    /// <summary>
    /// Forces the button to show, immune to spam clicks.
    /// (Spam týklamalara karþý baðýþýk olarak butonu zorla gösterir.)
    /// </summary>
    public void ShowButton()
    {
        isFolding = false;
        targetScale = originalScale;

        if (!gameObject.activeInHierarchy)
        {
            // Eðer tamamen kapalýysa, önce Y eksenini 0 yap ki ekranda sýfýrdan büyüyerek açýlsýn
            rectTransform.localScale = new Vector3(originalScale.x, 0f, originalScale.z);
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Forces the button to hide smoothly.
    /// (Butonu pürüzsüz bir þekilde gizlemeye zorlar.)
    /// </summary>
    public void HideButton()
    {
        isFolding = true;
        targetScale = new Vector3(originalScale.x, 0f, originalScale.z);
    }

    /// <summary>
    /// Smoothly interpolates the scale towards the target every frame.
    /// (Boyutu her karede hedefe doðru pürüzsüzce hesaplar.)
    /// </summary>
    private void Update()
    {
        // Spam týklamaya karþý ölümsüz motor (Coroutine içermez, asla kilitlenmez)
        // Zaman durduðunda (Time.timeScale = 0) bile çalýþmasý için unscaledDeltaTime kullanýyoruz.
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * foldSpeed);

        // Kapanma emri verildiyse ve boyutu neredeyse sýfýrlandýysa, objeyi tamamen kapat
        if (isFolding && Mathf.Abs(rectTransform.localScale.y) <= 0.01f)
        {
            rectTransform.localScale = targetScale;
            gameObject.SetActive(false);
        }
    }
}