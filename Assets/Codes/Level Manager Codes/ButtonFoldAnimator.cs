using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class ButtonFoldAnimator : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Fold Settings (Katlanma Ayarlarý)")]
    public float foldDuration = 0.2f; // Kapanma ve açýlma hýzý

    // Butonun asýl boyutu (orijinal hali)
    private Vector3 originalScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        // Buton aktif edildiðinde (SetActive(true)) açýlma animasyonunu baþlat
        StopAllCoroutines();
        StartCoroutine(UnfoldRoutine());
    }

    // Butonun kaybolmasý gerektiðinde bu fonksiyonu çaðýr
    public void HideButton()
    {
        StopAllCoroutines();
        StartCoroutine(FoldRoutine());
    }

    // --- GERÝ AÇILIRKEN ÇALIÞAN MOTOR ---
    private IEnumerator UnfoldRoutine()
    {
        float elapsed = 0f;

        // Baþlangýç: Y ekseni 0 (Tamamen ezik/katlanmýþ)
        Vector3 startScale = new Vector3(originalScale.x, 0f, originalScale.z);

        // Bitiþ: Orijinal boyut
        Vector3 endScale = originalScale;

        while (elapsed < foldDuration)
        {
            // Zaman dursa bile animasyon çalýþsýn
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / foldDuration;

            // "Ease Out" yumuþaklýðý (Hýzlý fýrlar, yerine otururken yavaþlar)
            float curve = 1f - (1f - t) * (1f - t);

            rectTransform.localScale = Vector3.Lerp(startScale, endScale, curve);

            yield return null;
        }

        rectTransform.localScale = endScale;
    }

    // --- KAPANIRKEN ÇALIÞAN MOTOR ---
    private IEnumerator FoldRoutine()
    {
        float elapsed = 0f;

        // Baþlangýç: Mevcut boyut
        Vector3 startScale = rectTransform.localScale;

        // Bitiþ: Y eksenini 0 yap (X ve Z ayný kalýyor, dikeyde eziliyor)
        Vector3 endScale = new Vector3(originalScale.x, 0f, originalScale.z);

        while (elapsed < foldDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / foldDuration;

            // "Ease In" yumuþaklýðý (Gittikçe hýzlanarak kapanýr)
            float curve = t * t;

            rectTransform.localScale = Vector3.Lerp(startScale, endScale, curve);

            yield return null;
        }

        rectTransform.localScale = endScale;

        // Animasyon bitince butonu tamamen kapat ki sahnede boþuna beklemesin
        gameObject.SetActive(false);
    }
}