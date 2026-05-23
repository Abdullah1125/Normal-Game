using UnityEngine;

/// <summary>
/// Manages player speed and keeps the target gate permanently open.
/// (Oyuncu hýzýný yönetir ve hedef kapýyý kalýcý olarak açýk tutar.)
/// </summary>
public class LowStance : MonoBehaviour
{
    [Header("Settings (Ayarlar)")]
    public float boostedSpeed = 20f;

    private GateController gate;
    private bool isGateOpened = false;
    private Vector3 cachedOpenPosition;

    /// <summary>
    /// Subscribes to the player reset event when the object is enabled.
    /// (Obje aktifleþtiðinde oyuncu sýfýrlanma event'ine abone olur.)
    /// </summary>
    private void OnEnable()
    {
        PlayerController.OnPlayerReset += ApplySpeedBoost;
    }

    /// <summary>
    /// Unsubscribes from events and resets player speed upon disable.
    /// (Kapanýþta event aboneliðini iptal eder ve oyuncu hýzýný sýfýrlar.)
    /// </summary>
    private void OnDisable()
    {
        PlayerController.OnPlayerReset -= ApplySpeedBoost;

        if (PlayerController.Instance != null)
            PlayerController.Instance.ResetSpeed();
    }

    /// <summary>
    /// Initializes references, opens the gate, and caches the target position.
    /// (Referanslarý alýr, kapýyý açar ve hedef pozisyonu önbelleðe alýr.)
    /// </summary>
    private void Start()
    {
        gate = GateController.Instance;
        ApplySpeedBoost();

        if (gate != null)
        {
            gate.OpenGate();
            isGateOpened = true;

            // Aðýr iþlem olan Reflection'ý sadece bir kere Start'ta yapýp kaydediyoruz.
            Vector3 startPos = (Vector3)typeof(GateController)
                .GetField("startPos", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(gate);

            cachedOpenPosition = startPos + gate.moveOffset;
        }
    }

    /// <summary>
    /// Applies the custom speed boost to the player.
    /// (Oyuncuya özel hýz takviyesini uygular.)
    /// </summary>
    private void ApplySpeedBoost()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.SetCustomSpeed(boostedSpeed);
        }
    }

    /// <summary>
    /// Locks the gate to the cached open position every frame.
    /// (Her karede kapýyý önbelleðe alýnmýþ açýk pozisyona kilitler.)
    /// </summary>
    private void LateUpdate()
    {
        // Artýk reflection yok, sadece bellekteki deðeri atýyoruz. CPU dostu.
        if (gate != null && isGateOpened)
        {
            gate.transform.position = cachedOpenPosition;
        }
    }
}