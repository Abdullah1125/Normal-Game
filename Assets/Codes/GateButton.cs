using UnityEngine;

public class GateButton : MonoBehaviour
{
    public GateController targetGate; // Butonun tetikleyeceði kapý objesi
    private Color originalColor;      // Butonun baþlangýç rengini saklar
    private Vector3 originalScale;    // Butonun baþlangýç boyutunu saklar
    private bool hasPressed = false;  // Butona basýlýp basýlmadýðýný takip eder

    void Awake()
    {
     
        originalColor = GetComponent<SpriteRenderer>().color;
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (!hasPressed && other.CompareTag("Player"))
        {
          
            if (LevelManager.Instance != null && !LevelManager.Instance.activeLevel.isButtonActive)
            {
                
                GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f);
                return;
            }

            hasPressed = true;

            // Kapý referansý varsa kapýyý aç
            if (targetGate != null) targetGate.OpenGate();

            // Görsel geri bildirim: Yeþil renk ve yassýlaþma efekti
            GetComponent<SpriteRenderer>().color = Color.green;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.6f, 1f);
        }
    }

    public void ResetButton()
    {
        // Butonu baþlangýç deðerlerine sýfýrla
        hasPressed = false;
        GetComponent<SpriteRenderer>().color = originalColor;
        transform.localScale = originalScale;
    }
}