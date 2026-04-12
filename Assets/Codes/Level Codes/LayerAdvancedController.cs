using UnityEngine;
using UnityEngine.Tilemaps;

public class LayerAdvancedController : MonoBehaviour
{
    private TilemapRenderer tr;
    private TilemapCollider2D tc;
    private LevelData lastLevel;

    void Awake()
    {
        tr = GetComponent<TilemapRenderer>();
        tc = GetComponent<TilemapCollider2D>();
    }

    void Update()
    {
        if (LevelManager.Instance == null || LevelManager.Instance.activeLevel == null) return;

       
        if (lastLevel != LevelManager.Instance.activeLevel)
        {
            lastLevel = LevelManager.Instance.activeLevel;
            ApplyAdvancedRules();
        }
    }

    void ApplyAdvancedRules()
    {
        // 1. ÇARPIÞMA (COLLIDER) KONTROLÜ
        // Eðer colliderLeyer kapalýysa, oyuncu içinden geçer 
        if (tc != null) tc.enabled = lastLevel.colliderLeyer;

        // 2. GÖRÜNÜRLÜK (RENDERER) KONTROLÜ
        // Eðer showLayer kapalýysa, Renderer'ý tamamen kapatýrýz.
        if (!lastLevel.showLayer)
        {
            if (tr != null) tr.enabled = false;
        }
        else
        {
            // Eðer görünür olmasý gerekiyorsa, baþlangýçta bir kez açýyoruz.
            if (tr != null) tr.enabled = true;
        }
    }
}