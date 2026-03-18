using UnityEngine;
using UnityEngine.Tilemaps;

public class BlindMemoryMechanic : MonoBehaviour
{
    // Renderer'larý bir kere sakla
    private TilemapRenderer[] cachedRenderers;

    void Awake()
    {
        // Sadece bir kere bul ve sakla
        GameObject levelGrid = GameObject.Find("Grid");
        if (levelGrid != null)
        {
            cachedRenderers = levelGrid.GetComponentsInChildren<TilemapRenderer>();
        }
        SetGridVisibility(false);
    }

    private void OnDisable()
    {
        SetGridVisibility(true);
    }

    private void SetGridVisibility(bool isVisible)
    {
        // Artýk Find yok, saklanan diziyi kullan
        if (cachedRenderers == null) return;

        foreach (TilemapRenderer r in cachedRenderers)
        {
            if (r != null)
            {
                r.enabled = isVisible;
            }
        }
    }
}