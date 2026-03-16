using UnityEngine;

public class CameraRoomController : MonoBehaviour
{

    public float transitionSpeed = 5f; // Kameranýn kayma hýzý
    private Vector3 mainRoomPos;       // Ana oda konumu
    private Vector3 targetPos;         // Gidilecek hedef konum

    void Awake()
    {
       
        mainRoomPos = transform.position;
        targetPos = mainRoomPos;
    }

    void Update()
    {
        // Kamerayý hedef konuma yumuþakça kaydýr
        transform.position = Vector3.Lerp(transform.position, targetPos, transitionSpeed * Time.deltaTime);
    }

    public void ChangeRoom(bool isInSecretPassage)
    {
        LevelData data = LevelManager.Instance.activeLevel;

        if (data != null && data.hasSecretPassage)
        {
            // Doðru odayý seç: Gizli geçitteyse secretRoom, deðilse mainRoom
            targetPos = isInSecretPassage ? data.secretRoomPos : mainRoomPos;
        }
    }

    public void ResetCamera()
    {
        // Hedefi baþlangýç konumuna döndür
        targetPos = mainRoomPos;
    }
}