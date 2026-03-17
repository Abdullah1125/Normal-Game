using UnityEngine;

public class CameraRoomController : MonoBehaviour
{
    public static CameraRoomController Instance;

    public float transitionSpeed = 5f;
    private Vector3 mainRoomPos;
    private Vector3 targetPos;

    void Awake()
    {
        Instance = this;
        mainRoomPos = transform.position; // Kameranýn sahnedeki orijinal yeri
        targetPos = mainRoomPos;
    }

    void Update()
    {
        // Kamerayý hedef konuma yumuþakça kaydýr
        transform.position = Vector3.Lerp(transform.position, targetPos, transitionSpeed * Time.deltaTime);
    }

    // Prefablar bu fonksiyonu çaðýracak
    public void SetTargetPosition(Vector3 newPos)
    {
        targetPos = newPos;
    }

    public void ResetCamera()
    {
        targetPos = mainRoomPos;
    }
}