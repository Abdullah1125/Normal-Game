using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera shake and hit-stop effects. Camera movement/transition has been removed.
/// (Kamera sarsýntýsý ve vuruþ donmasýný yönetir. Kamera hareketi/geçiþi kaldýrýlmýþtýr.)
/// </summary>
public class CameraRoomController : MonoBehaviour
{
    public static CameraRoomController Instance;

    [Header("Death Shake Settings (Ölüm Tokadý)")]
    public float shakeDuration = 0.35f;
    public float shakeMagnitude = 0.6f;
    public float shakeSpeed = 45f;
    private Vector3 shakeOffset;

    [Header("Impact Settings (Çarpýþma Hissi)")]
    // Ölüm anýnda oyunun saliselik donma süresi (Hit Stop).
    public float hitStopDuration = 0.05f;

    // Kameranýn baþlangýçtaki sabit pozisyonu
    private Vector3 basePosition;

    void Awake()
    {
        if (Instance == null) Instance = this;
        basePosition = transform.position; // Baþlangýç yerini kaydet
    }

    void LateUpdate()
    {
        // Kamera artýk bir yere gitmiyor, sadece ana pozisyonunda durup gerekirse titriyor
        transform.position = basePosition + shakeOffset;
    }

    // --- SARSINTI MEKANÝZMASI ---
    public void ShakeCamera()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        // 1. AÞAMA: HIT STOP (ZAMAN DONMASI)
        if (hitStopDuration > 0f)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
        }

        // 2. AÞAMA: KÜBÝK SARSINTI (BALYOZ ETKÝSÝ)
        float elapsed = 0.0f;
        float randomStart = Random.Range(-1000f, 1000f);

        while (elapsed < shakeDuration)
        {
            float percentComplete = elapsed / shakeDuration;
            float damping = Mathf.Pow(1.0f - percentComplete, 3f);

            float x = (Mathf.PerlinNoise(randomStart + Time.unscaledTime * shakeSpeed, 0f) - 0.5f) * 2f;
            float y = (Mathf.PerlinNoise(0f, randomStart + Time.unscaledTime * shakeSpeed) - 0.5f) * 2f;

            shakeOffset = new Vector3(x, y, 0f) * shakeMagnitude * damping;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
    }
}