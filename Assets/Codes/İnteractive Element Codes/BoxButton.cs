using UnityEngine;

public class BoxButton : MonoBehaviour
{
    private SpriteRenderer sr;
    private bool isPressed = false;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    private void OnTriggerEnter2D(Collider2D other)
    {
       
        if (Box.Instance != null && other.gameObject == Box.Instance.gameObject)
        {
            if (!isPressed)
            {
                isPressed = true;
                sr.color = Color.green;
                GateController.Instance?.OpenGate(); // Kapıyı aç
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (Box.Instance != null && other.gameObject == Box.Instance.gameObject)
        {
            if (isPressed)
            {
                isPressed = false;
                sr.color = Color.white;
                GateController.Instance?.CloseGate(); // Kapıyı kapat
            }
        }
    }
    public void ResetButton()
    {
        isPressed = false;
        gameObject.SetActive(true);
        sr.color = Color.white;
        
    }
}