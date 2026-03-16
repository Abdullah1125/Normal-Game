using UnityEngine;

public class SecretAreaTrigger : MonoBehaviour
{
    private bool isCameraInSecretRoom = false; // Kameranýn gizli odada olup olmadýðýný tutar

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            
            isCameraInSecretRoom = !isCameraInSecretRoom;

            // Kamera kontrolcüsünü bul ve yeni oda durumunu bildir
            CameraRoomController cameraController = FindFirstObjectByType<CameraRoomController>();
            if (cameraController != null)
            {
                cameraController.ChangeRoom(isCameraInSecretRoom);
            }

            Debug.Log("Kamera modu deðiþtirildi: " + (isCameraInSecretRoom ? "Gizli Oda" : "Ana Oda"));
        }
    }

    // Seviye sýfýrlandýðýnda veya karakter öldüðünde durumu baþlangýca döndür
    public void ResetTrigger()
    {
        isCameraInSecretRoom = false;
    }
}