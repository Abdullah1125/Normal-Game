using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class AirplaneUI : MonoBehaviour
{
    public static AirplaneUI Instance;

    private CanvasGroup canvasGroup;

    [Header("UI Speed ​​Settings(UI Hız Ayarları)")]
    public float fadeInSpeed = 5.0f;   // Çok yavaş değil, tatlı bir hızda belirir
    public float fadeOutSpeed = 3.0f;  // Çıkınca biraz daha yavaş ve süzülerek kaybolur

    private bool isVisible = false;

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        HideImmediately();
    }

    private void Update()
    {
        // 1. Görünür olması gerekiyorsa ve henüz tam dolmadıysa:
        if (isVisible && canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeInSpeed;
        }
        // 2. Gizlenmesi gerekiyorsa ve henüz tam silinmediyse:
        else if (!isVisible && canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;

            // Tamamen silindiğinde objeyi kapat ki performansı yormasın
            if (canvasGroup.alpha <= 0f)
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void ShowPanel()
    {
        isVisible = true;
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        isVisible = false;
    }

    public void HideImmediately()
    {
        isVisible = false;
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}