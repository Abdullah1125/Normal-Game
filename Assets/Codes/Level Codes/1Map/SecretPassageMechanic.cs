using UnityEngine;

public class SecretPassageMechanic : MonoBehaviour
{
    [Header("Duvar Ayarlarý")]
    public string targetObjectName = "Tilemap_Secret";

    [Header("Kamera Ayarlarý")]
    public Vector3 secretRoomPos;

    private GameObject secretWall;
    private Collider2D wallCollider;

    void Start()
    {
        secretWall = GameObject.Find(targetObjectName);
        if (secretWall != null)
        {
            wallCollider = secretWall.GetComponent<Collider2D>();
            if (wallCollider != null) wallCollider.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gizli odaya git
            if (CameraRoomController.Instance != null)
                CameraRoomController.Instance.SetTargetPosition(secretRoomPos);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Ana odaya dön
            if (CameraRoomController.Instance != null)
                CameraRoomController.Instance.ResetCamera();
        }
    }

    private void OnDisable()
    {
        // OnDestroy yerine OnDisable kullan - daha güvenli
        if (wallCollider != null) wallCollider.enabled = true;
    }
}