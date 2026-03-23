using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

public class BoxButton : MonoBehaviour
{
    [Header("Ayarlar")]
    public Color normalColor = Color.white;
    public Color pressedColor = Color.green;

    private SpriteRenderer sr;
    private bool isPressed = false;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = normalColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (IsBox(other) && !isPressed) 
        {
            PressButton();
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {

        if (IsBox(other) && !isPressed)
        {
            PressButton();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsBox(other) && isPressed)
        {
            ReleaseButton();
        }
    }
    private bool IsBox (Collider2D other)
    {
        if (Box.Instance != null && other.gameObject == Box.Instance.gameObject) return true;
        if (other.GetComponent<Box>() != null ) return true;
        return false;
    }

    private void PressButton()
    {
        isPressed = true;
        if (sr != null) sr.color = pressedColor;
        GateController.Instance?.OpenGate();
        Debug.Log("Buton aktif!");
    }

    private void ReleaseButton ()
    {
        isPressed = false;
        if (sr != null) sr.color = normalColor;
        GateController.Instance?.CloseGate();
        Debug.Log("Buton deaktif!");
    }
    public void ResetButton()
    {
        isPressed = false;
        gameObject.SetActive(true);
        if (sr != null) sr.color = normalColor;

    }
}