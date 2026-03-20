using UnityEngine;

public class GateButton : MonoBehaviour
{
    
    private Color originalColor;      // Butonun baþlangýç rengini saklar
    private Vector3 originalScale;    // Butonun baþlangýç boyutunu saklar
    private bool hasPressed = false;  // Butona basýlýp basýlmadýðýný takip eder
    private SpriteRenderer sr;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalScale = transform.localScale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (!hasPressed && other.CompareTag("Player"))
        {
            
            if (LevelManager.Instance != null && !LevelManager.Instance.activeLevel.isActive)
            {
                sr.color = new Color(0.3f, 0.3f, 0.3f);
                return;
            }

            GravityButtonTrigger gravityTrigger = GetComponent<GravityButtonTrigger>();
            if (gravityTrigger != null)
            {
                gravityTrigger.ExecuteFlip();
            }

            hasPressed = true;

            // Kapý referansý varsa kapýyý aç
            if (GateController.Instance != null)
            {
                GateController.Instance.OpenGate();
            }
            else
            {
                Debug.LogError("Sahnede GateController bulunamadý!");
            }

            // Görsel geri bildirim: Yeþil renk ve yassýlaþma efekti
            sr.color = Color.green;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * 0.6f, 1f);
        }
    }
    public void ResetButton()
    {
        hasPressed = false;
        sr.color = Color.white;
        gameObject.SetActive(true);
    }

}