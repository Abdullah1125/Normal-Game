using System.Collections;
using UnityEngine;

/// <summary>
/// Automatically resizes the background object to fill the screen or canvas.
/// (Arka plan objesini ekranı veya canvas'ı kaplayacak şekilde otomatik esnetir.)
/// </summary>
public class AutoFullScreenBackground : MonoBehaviour
{
    [Header("Timing Settings (Zamanlama Ayarları)")]
    public float expandDelay = 0.5f;

    private GameObject bgObject;

    //Sürekli aranmaması için tanımlanmış önbellek (Cache) değişkenleri
    private RectTransform bgRect;
    private SpriteRenderer bgSpriteRenderer;
    private Canvas rootCanvas;
    private Camera mainCam;

    private bool isModified = false;

    // UI and Sprite memory variables (UI ve Sprite hafıza değişkenleri)
    private Vector2 origAnchorMin, origAnchorMax, origPivot, origAnchoredPos, origSizeDelta;
    private Vector3 origScale, origPos, origSpriteScale;

    /// <summary>
    /// Caches global references like Camera before starting.
    /// (Başlamadan önce Kamera gibi global referansları önbelleğe alır.)
    /// </summary>
    private void Awake()
    {
        mainCam = Camera.main; // Kamera genelde sabittir, baştan bulup cebe atıyoruz.
    }

    /// <summary>
    /// Starts the delayed fullscreen routine when the object becomes active.
    /// (Obje sahnede aktif olduğunda gecikmeli tam ekran rutinine başlar.)
    /// </summary>
    private void Start()
    {
        StartCoroutine(DelayedExpandRoutine());
    }

    /// <summary>
    /// Automatically triggered when LevelManager sends "OnLevelStarted" signal.
    /// (LevelManager "OnLevelStarted" sinyalini verdiğinde otomatik tetiklenir.)
    /// </summary>
    private void OnEnable()
    {
        LevelManager.OnLevelStarted += ReapplyFullScreen;
    }

    /// <summary>
    /// Stops listening to signals and restores original state on disable.
    /// (Obje kapandığında veya silindiğinde sinyal dinlemeyi bırakır ve eski haline döner.)
    /// </summary>
    private void OnDisable()
    {
        LevelManager.OnLevelStarted -= ReapplyFullScreen;
        RestoreOriginalState();
    }

    /// <summary>
    /// Waits for the specified delay, then triggers the expand function.
    /// (Belirlenen süre kadar bekler ve ardından ekranı kaplama fonksiyonunu tetikler.)
    /// </summary>
    private IEnumerator DelayedExpandRoutine()
    {
        yield return new WaitForSeconds(expandDelay);
        SetSizeToScreenByTag();
    }

    /// <summary>
    /// Finds the target background object and stretches it to screen dimensions.
    /// (Hedef arka plan objesini bulup ekran boyutlarına göre esnetir.)
    /// </summary>
    public void SetSizeToScreenByTag()
    {
        GameObject currentBg = BackgroundIdentity.Instance;
        if (currentBg == null) return; // Çökmeyi önle

        // JİLET: Arka plan objesi değiştiyse veya ilk defa alınıyorsa bileşenleri 1 kere bul!
        if (bgObject != currentBg)
        {
            bgObject = currentBg;
            bgRect = bgObject.GetComponent<RectTransform>();
            bgSpriteRenderer = bgObject.GetComponent<SpriteRenderer>();

            if (bgRect != null)
            {
                // Parent araması ağırdır, sadece 1 kere yapılır
                Canvas parentCanvas = bgObject.GetComponentInParent<Canvas>();
                if (parentCanvas != null) rootCanvas = parentCanvas.rootCanvas;
            }
        }

        // --- UI (Canvas) Arka Planı İşlemleri ---
        if (bgRect != null)
        {
            if (!isModified)
            {
                origAnchorMin = bgRect.anchorMin;
                origAnchorMax = bgRect.anchorMax;
                origPivot = bgRect.pivot;
                origAnchoredPos = bgRect.anchoredPosition;
                origSizeDelta = bgRect.sizeDelta;
                origScale = bgRect.localScale;
                isModified = true;
            }

            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;

            if (rootCanvas != null)
            {
                RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(canvasRect.rect.width, canvasRect.rect.height);
            }
            else
            {
                bgRect.sizeDelta = new Vector2(Screen.width, Screen.height);
            }
            bgRect.localScale = Vector3.one;
        }
        // --- 2D Sprite Arka Planı İşlemleri ---
        else if (bgSpriteRenderer != null)
        {
            if (!isModified)
            {
                origPos = bgObject.transform.position;
                origSpriteScale = bgObject.transform.localScale;
                isModified = true;
            }

            if (mainCam != null && mainCam.orthographic)
            {
                bgObject.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, bgObject.transform.position.z);

                float cameraHeight = mainCam.orthographicSize * 2f;
                float cameraWidth = cameraHeight * mainCam.aspect;

                //Arama yok, direkt önbellekten sprite boyutlarını çek
                float spriteHeight = bgSpriteRenderer.sprite.bounds.size.y;
                float spriteWidth = bgSpriteRenderer.sprite.bounds.size.x;

                // Ekranı tam kaplaması için genişlik ve yükseklik oranlarını ayrı ayrı uyguluyoruz (Orijinal esnetme yöntemi)
                float scaleX = cameraWidth / spriteWidth;
                float scaleY = cameraHeight / spriteHeight;

                bgObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }
    }

    /// <summary>
    /// Restores the background to its original state.
    /// (Obje devre dışı kaldığında arka planı eski haline döndürür.)
    /// </summary>
    public void RestoreOriginalState()
    {
        if (!isModified || bgObject == null) return;

        if (bgRect != null)
        {
            bgRect.anchorMin = origAnchorMin;
            bgRect.anchorMax = origAnchorMax;
            bgRect.pivot = origPivot;
            bgRect.anchoredPosition = origAnchoredPos;
            bgRect.sizeDelta = origSizeDelta;
            bgRect.localScale = origScale;
        }
        else if (bgSpriteRenderer != null)
        {
            bgObject.transform.position = origPos;
            bgObject.transform.localScale = origSpriteScale;
        }

        isModified = false;
    }

    /// <summary>
    /// Adds a slight delay before going full screen when a new level starts.
    /// (Yeni level başladığında araya küçük bir gecikme koyup tam ekran yapar.)
    /// </summary>
    private void ReapplyFullScreen()
    {
        StopAllCoroutines(); // Olası çakışmaları engeller
        StartCoroutine(DelayedExpandRoutine());
    }
}