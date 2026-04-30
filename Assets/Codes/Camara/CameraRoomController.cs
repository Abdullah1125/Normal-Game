using System.Collections;
using UnityEngine;

/// <summary>
/// Handles camera shake and hit-stop effects. Camera movement/transition has been removed.
/// (Kamera sarsıntısı ve vuruş donmasını yönetir. Kamera hareketi/geçişi kaldırılmıştır.)
/// </summary>
public class CameraRoomController : Singleton<CameraRoomController>
{

    [Header("Death Shake Settings (Ölüm Tokadı)")]
    public float shakeDuration = 0.35f;
    public float shakeMagnitude = 0.6f;
    public float shakeSpeed = 45f;
    private Vector3 shakeOffset;

    [Header("Impact Settings (Çarpışma Hissi)")]
    // Ölüm anında oyunun saliselik donma süresi (Hit Stop).
    public float hitStopDuration = 0.05f;

    // Kameranın başlangıçtaki sabit pozisyonu
    private Vector3 basePosition;

    protected override void Awake()
    {
        base.Awake();
        basePosition = transform.position; // Başlangıç yerini kaydet
    }

    void LateUpdate()
    {
        // Kamera artık bir yere gitmiyor, sadece ana pozisyonunda durup gerekirse titriyor
        transform.position = basePosition + shakeOffset;
    }

    // --- SARSINTI MEKANİZMASI ---
    public void ShakeCamera()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        // 1. AŞAMA: HIT STOP (ZAMAN DONMASI)
        if (hitStopDuration > 0f)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
        }

        // 2. AŞAMA: KÜBİK SARSINTI (BALYOZ ETKİSİ)
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
