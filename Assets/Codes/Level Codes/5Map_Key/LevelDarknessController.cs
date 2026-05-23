using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls the screen darkness dynamically based on the player's X coordinate.
/// (Oyuncunun X koordinatýna göre ekran karanlýðýný dinamik olarak ayarlar.)
/// </summary>
public class LevelMasterDarkness : MonoBehaviour
{
    [Header("Coordinate Settings (Koordinat Ayarlarý)")]
    public float startX = 0f;
    public float endX = 100f;

    [Header("Visual Settings (Görsel Ayarlar)")]
    public float targetExposure = -3f;

    private Transform player;
    private Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    // Optimizasyon: Gereksiz atamalarý önlemek için son deðeri saklarýz
    private float lastProgress = -1f;

    /// <summary>
    /// Finds references and sets the initial exposure to zero.
    /// (Referanslarý bulur ve baþlangýç aydýnlýk deðerini sýfýrlar.)
    /// </summary>
    private void Start()
    {
        if (PlayerController.Instance != null)
            player = PlayerController.Instance.transform;

        globalVolume = Object.FindFirstObjectByType<Volume>();

        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.postExposure.value = 0f;
        }
    }

    /// <summary>
    /// Calculates the progress based on position and applies the exposure if changed.
    /// (Pozisyona göre ilerlemeyi hesaplar ve deðiþtiyse karanlýk deðerini uygular.)
    /// </summary>
    private void LateUpdate()
    {
        if (player == null || colorAdjustments == null) return;

        float progress = Mathf.InverseLerp(startX, endX, player.position.x);

        // MOBÝL OPTÝMÝZASYON: Sadece oyuncu hareket ettiðinde ve deðer deðiþtiðinde uygula
        if (Mathf.Abs(progress - lastProgress) > 0.001f)
        {
            colorAdjustments.postExposure.value = Mathf.Lerp(0f, targetExposure, progress);
            lastProgress = progress;
        }
    }

    /// <summary>
    /// Resets the exposure effect when the object is destroyed or the level changes.
    /// (Obje yok edildiðinde veya level deðiþtiðinde karanlýk efektini sýfýrlar.)
    /// </summary>
    private void OnDestroy()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = 0f;
        }
    }
}