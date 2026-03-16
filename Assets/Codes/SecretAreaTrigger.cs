using UnityEngine;

public class SecretAreaTrigger : MonoBehaviour
{
    private bool kameraGizliOdada = false; // Kameranýn nerede olduðunu hatýrla

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Durumu tersine çevir (true ise false, false ise true yap)
            kameraGizliOdada = !kameraGizliOdada;

            Debug.Log("Kamera Modu Deðiþti: " + (kameraGizliOdada ? "Gizli Oda" : "Ana Oda"));

            // Kameraya yeni durumu gönder
            FindFirstObjectByType<CameraRoomController>().OdayiDegistir(kameraGizliOdada);
        }
    }

    // Level atlandýðýnda veya karakter ölünce bu deðiþkeni sýfýrlamak için fonksiyon
    public void ResetTrigger()
    {
        kameraGizliOdada = false;
    }
}