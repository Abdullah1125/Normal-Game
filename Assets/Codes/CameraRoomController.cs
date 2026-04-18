using System.Collections;
using UnityEngine;

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

    [Header("Transition Settings(Geçiþ Ayarlarý)")]
    public float transitionSpeed = 5f;
    private Vector3 mainRoomPos;
    private Vector3 targetPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        mainRoomPos = transform.position;
        targetPos = mainRoomPos;
    }

    void Update()
    {
        Vector3 nextPos = Vector3.Lerp(transform.position, targetPos, transitionSpeed * Time.deltaTime);
        transform.position = nextPos + shakeOffset;
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
        // Karakter öldüðü an oyun saliselik buz keser. Ekran titreþmeden önce bu donma, çarpmanýn þiddetini beynimize iþler.
        if (hitStopDuration > 0f)
        {
            Time.timeScale = 0f; // Oyunu dondur
            yield return new WaitForSecondsRealtime(hitStopDuration); // Gerçek zamanda çok kýsa bir süre bekle
            Time.timeScale = 1f; // Oyunu geri akýt
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

    public void SetTargetPosition(Vector3 newPos)
    {
        targetPos = newPos;
    }

    public void ResetCamera()
    {
        StopAllCoroutines();
        shakeOffset = Vector3.zero;
        targetPos = mainRoomPos;
        Time.timeScale = 1f; // Ne olur ne olmaz, sýfýrlanýnca zamanýn aktýðýndan emin olalým
    }
}