using UnityEngine;

public class SoulEffect : MonoBehaviour
{
    public float lifeTime = 3f;           // Objelerin yok edilmeden önceki bekleme süresi
    private bool hasTouchedGround = false; // Yere temas edilip edilmediði kontrolü

    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        if (!hasTouchedGround && (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground")))
        {
            hasTouchedGround = true;

          
            Destroy(gameObject, lifeTime);
        }
    }
}