using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitter : MonoBehaviour
{
    [Header("Resolution Settings")]
    [Tooltip("Target horizontal width in units. (Birim cinsinden hedeflenen yatay geniþlik.)")]
    public float targetWidth = 20f;

    private Camera cam;

    /// <summary>
    /// Initializes the camera reference and triggers the size adjustment.
    /// (Kamera referansýný baþlatýr ve boyut ayarlamasýný tetikler.)
    /// </summary>
    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCameraSize();
    }

    /// <summary>
    /// Calculates and sets the orthographic size to maintain a fixed horizontal field of view.
    /// (Yatay görüþ alanýný sabit tutmak için ortografik boyutu hesaplar ve uygular.)
    /// </summary>
    void AdjustCameraSize()
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;

        // Cihazýn oranýna göre kameranýn zoom (Orthographic Size) deðerini ayarla
        cam.orthographicSize = targetWidth / (2f * screenRatio);
    }
}