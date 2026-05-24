using UnityEngine;
using System.Reflection;

/// <summary>
/// GateController'ýn gizli verilerine sýzan ve pozisyonlarý hesaplayan ortak modüler araç.
/// </summary>
public static class GateUtility
{
    private static FieldInfo _startPosField;
    private static bool _isInitialized = false;

    /// <summary>
    /// Reflection'ý sadece bir kere çalýþtýrýp hafýzaya alýr.
    /// </summary>
    private static void Initialize()
    {
        if (!_isInitialized)
        {
            _startPosField = typeof(GateController).GetField("startPos", BindingFlags.NonPublic | BindingFlags.Instance);
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Kapýnýn kapalý (orijinal) pozisyonunu döndürür.
    /// </summary>
    public static Vector3 GetClosedPosition()
    {
        Initialize();
        if (GateController.Instance == null || _startPosField == null) return Vector3.zero;

        return (Vector3)_startPosField.GetValue(GateController.Instance);
    }

    /// <summary>
    /// Kapýnýn açýk olmasý gereken hedef pozisyonunu döndürür.
    /// </summary>
    public static Vector3 GetOpenPosition()
    {
        if (GateController.Instance == null) return Vector3.zero;

        return GetClosedPosition() + GateController.Instance.moveOffset;
    }

    /// <summary>
    /// Kapýyý animasyonsuz olarak anýnda açýk pozisyona ýþýnlar.
    /// </summary>
    public static void SnapGateOpen()
    {
        if (GateController.Instance != null)
        {
            GateController.Instance.OpenGate();
            GateController.Instance.transform.position = GetOpenPosition();
        }
    }
}