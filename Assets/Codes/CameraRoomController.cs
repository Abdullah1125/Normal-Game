using System.Collections;
using UnityEngine;

public class CameraRoomController : MonoBehaviour
{
    public static CameraRoomController Instance;

    [Header("Shake Settings")]
    public float shakeDuration = 0.1f;   // Sarsýntý süresi
    public float shakeMagnitude = 0.1f;  // Sarsýntý þiddeti
    private Vector3 shakeOffset;         // Sarsýntýdan kaynaklanan kayma miktarý

    [Header("Transition Settings")]
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
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            // Rastgele küçük yönlere ofset belirle
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            shakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null; // Bir sonraki kareye kadar bekle
        }

        // Sarsýntý bitince ofseti sýfýrla
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
    }
}