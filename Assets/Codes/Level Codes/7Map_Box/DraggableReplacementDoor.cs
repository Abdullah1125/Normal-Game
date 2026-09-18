using UnityEngine;

public class DraggableReplacementDoor : MonoBehaviour
{
    [Header("Tag Ayarları")]
    public string oldDoorTag = "OldDoor";

    [Header("Yükseklik Sınırı Ayarları")]
    public float maxYPosition = 5f;

    private float minYPosition;
    private Vector3 initialPosition;

    private Vector3 offset;
    private float zCoord;
    private GameObject foundOldDoor;


    private void OnEnable()
    {
        PlayerController.OnPlayerReset += ResetDoorPosition;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerReset -= ResetDoorPosition;
    }

    void Start()
    {
        foundOldDoor = GameObject.FindGameObjectWithTag(oldDoorTag);
        if (foundOldDoor != null)
        {
            transform.position = foundOldDoor.transform.position;
            foundOldDoor.SetActive(false);
        }

        initialPosition = transform.position;
        minYPosition = transform.position.y;
    }

    void OnMouseDown()
    {
        zCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        offset = gameObject.transform.position - GetMouseAsWorldPoint();
    }

    void OnMouseDrag()
    {
        Vector3 mouseWorldPos = GetMouseAsWorldPoint() + offset;
        Vector3 newPos = transform.position;

        newPos.y = mouseWorldPos.y;

        if (newPos.y < minYPosition)
        {
            newPos.y = minYPosition;
        }

        if (newPos.y > maxYPosition)
        {
            newPos.y = maxYPosition;
        }

        transform.position = newPos;
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

  
    private void ResetDoorPosition()
    {
        transform.position = initialPosition;
        
    }

    private void OnDestroy()
    {
       
        if (foundOldDoor != null)
        {
            foundOldDoor.SetActive(true);
            
        }
    }
}
