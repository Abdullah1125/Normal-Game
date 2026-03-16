using UnityEngine;

public class Key : MonoBehaviour
{
    public GateController targetGate; 


    private void OnTriggerEnter2D(Collider2D other)
    {
       
        if (other.CompareTag("Player"))
        {
         
            if (targetGate != null) targetGate.OpenGate();

            
      

       
            gameObject.SetActive(false);

            Debug.Log("Anahtar alýndý, kapý açýlýyor!");
        }
    }


    public void ResetKey()
    {
        gameObject.SetActive(true);
    }
}