using System.Collections;
using UnityEngine;

public class AutoFullScreenBackground : MonoBehaviour
{
    [Header("Timing Settings(Zamanlama Ayarlarý)")]
    public float expandDelay = 0.5f;

    private GameObject bgObject;
    private RectTransform bgRect;
    private bool isModified = false;

    // UI ve Sprite hafýza deðiþkenleri
    private Vector2 origAnchorMin, origAnchorMax, origPivot, origAnchoredPos, origSizeDelta;
    private Vector3 origScale, origPos, origSpriteScale;

    /// <summary>
    /// Obje sahnede aktif oldugunda gecikmeli tam ekran rutinine baslar.
    /// </summary>
    void Start()
    {
        StartCoroutine(DelayedExpandRoutine());
    }

    /// <summary>
    /// Belirlenen sure kadar bekler ve ardindan ekrani kaplama fonksiyonunu tetikler.
    /// </summary>
    private IEnumerator DelayedExpandRoutine()
    {
        yield return new WaitForSeconds(expandDelay);
        SetSizeToScreenByTag();
    }

    /// <summary>
    /// "Background" etiketli objeyi bulup ekran boyutlarina gore esnetir.
    /// </summary>
    public void SetSizeToScreenByTag()
    {
        bgObject = GameObject.FindGameObjectWithTag("Background");

        // Eger obje o sirada silinmisse islem yapma, cokmeyi onle
        if (bgObject == null) return;

        bgRect = bgObject.GetComponent<RectTransform>();

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

            Canvas rootCanvas = bgObject.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                RectTransform canvasRect = rootCanvas.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(canvasRect.rect.width + 10f, canvasRect.rect.height + 10f);
            }
            else
            {
                bgRect.sizeDelta = new Vector2(Screen.width + 10f, Screen.height + 10f);
            }
            bgRect.localScale = Vector3.one;
        }
        else if (bgObject.GetComponent<SpriteRenderer>() != null)
        {
            if (!isModified)
            {
                origPos = bgObject.transform.position;
                origSpriteScale = bgObject.transform.localScale;
                isModified = true;
            }

            SpriteRenderer sr = bgObject.GetComponent<SpriteRenderer>();
            Camera cam = Camera.main;

            if (cam != null && cam.orthographic)
            {
                bgObject.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, bgObject.transform.position.z);
                float cameraHeight = cam.orthographicSize * 2f;
                float cameraWidth = cameraHeight * cam.aspect;
                float spriteHeight = sr.sprite.bounds.size.y;
                float spriteWidth = sr.sprite.bounds.size.x;

                bgObject.transform.localScale = new Vector3((cameraWidth / spriteWidth) * 1.02f, (cameraHeight / spriteHeight) * 1.02f, 1f);
            }
        }
    }

    /// <summary>
    /// Obje devre disi kaldiginda arka plani eski haline dondurur.
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
        else
        {
            bgObject.transform.position = origPos;
            bgObject.transform.localScale = origSpriteScale;
        }

        isModified = false;
    }

    /// <summary>
    /// LevelManager "OnLevelStarted" sinyalini verdiginde otomatik tetiklenir.
    /// </summary>
    void OnEnable()
    {
        // Eger LevelManager varsa, yeni level basladiginda bizim fonksiyonu cagir!
        LevelManager.OnLevelStarted += ReapplyFullScreen;
    }

    /// <summary>
    /// Obje kapandiginda veya silindiginde sinyal dinlemeyi birakir.
    /// </summary>
    void OnDisable()
    {
        LevelManager.OnLevelStarted -= ReapplyFullScreen;
        RestoreOriginalState(); // Orijinal haline donme kodun zaten buradaydi
    }

    /// <summary>
    /// Yeni level basladiginda araya kucuk bir gecikme koyup tam ekran yapar.
    /// (Objelerin yaratilma suresi icin gecikme sarttir).
    /// </summary>
    private void ReapplyFullScreen()
    {
        StopAllCoroutines(); // Olasý cakismalari engeller
        StartCoroutine(DelayedExpandRoutine());
    }
}