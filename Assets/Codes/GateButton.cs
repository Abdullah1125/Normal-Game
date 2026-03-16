using UnityEngine;

public class GateButton : MonoBehaviour
{
    public GateController targetGate;
    private Color originalColor;
    private Vector3 originalScale;
    private bool hasPressed = false;

    void Awake()
    {
        originalColor = GetComponent<SpriteRenderer>().color;
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasPressed && other.CompareTag("Player"))
        {
            hasPressed = true;
            if (targetGate != null) targetGate.OpenGate();
            GetComponent<SpriteRenderer>().color = Color.green;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.6f, 1f);
        }
    }


    public void ResetButton()
    {
        hasPressed = false;
        GetComponent<SpriteRenderer>().color = originalColor;
        transform.localScale = originalScale;
    }
}