using UnityEngine;

public class InstantGateBypass : MonoBehaviour
{
    /// <summary>
    /// Obje sahnede var olduðu an GateController'ýn fiziksel engelini kaldýrýr.
    /// </summary>
    private void Awake()
    {
        if (GateController.Instance != null)
        {
            Collider2D gateCollider = GateController.Instance.GetComponent<Collider2D>();

            if (gateCollider != null)
            {
                gateCollider.enabled = false; // Çarpýþmayý iptal et
            }

            // Not: Eðer aþaðýdaki kod aktif edilirse GateController içindeki ses efektini de tetikler!
            // GateController.Instance.OpenGate(); 
        }
    }

    /// <summary>
    /// Obje sahneden silindiðinde GateController'ýn fiziksel engelini tekrar aktif eder.
    /// </summary>
    private void OnDestroy()
    {
        // Sahne kapanýrken (uygulama çýkýþýnda) null referans hatasý almamak için kontrol ediyoruz
        if (GateController.Instance != null)
        {
            Collider2D gateCollider = GateController.Instance.GetComponent<Collider2D>();

            if (gateCollider != null)
            {
                gateCollider.enabled = true; // Çarpýþmayý geri aç
            }
        }
    }
}