using UnityEngine;

/// <summary>
/// Arayüzü telefonlarýn kamera delikleri ve çentiklerinden korur.
/// (Protects the UI from phone camera cutouts and notches.)
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        panel = GetComponent<RectTransform>();
        Refresh();
    }

    void Update()
    {
        if (panel != null && Screen.safeArea != lastSafeArea)
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        // Ekranýn safe area koordinatlarýný Anchor deðerlerine çevir
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}