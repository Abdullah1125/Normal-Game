using UnityEngine;

public class Key : MonoBehaviour
{
    public GateController targetGate;

    private void OnTriggerEnter2D(Collider2D other)
    {
      
        if (other.CompareTag("Player"))
        {
          
            if (targetGate != null)
            {
                targetGate.OpenGate();
            }

            
            gameObject.SetActive(false);

            Debug.Log("Key collected, gate is opening!");
        }
    }

    // Seviye yeniden baþladýðýnda anahtarý tekrar görünür yapmak için kullanýlýr
    public void ResetKey()
    {
        gameObject.SetActive(true);
    }
}