using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FastFallTrigger : MonoBehaviour
{
    [Header("Düşüş Hızı")]
    public float fallSpeed = -30f; 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                
                playerRb.linearVelocity = new Vector2(0f, fallSpeed);
            }

           
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.SetCustomSpeed(0f);
            }
        }
    }

   
}
