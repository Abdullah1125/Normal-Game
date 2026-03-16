using UnityEngine;
public class SoulEffect : MonoBehaviour
{

    public float lifeTime = 3f;
    private bool hasTouchedGround = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasTouchedGround && (collision.gameObject.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground")))
        {

            hasTouchedGround = true;
            Destroy(gameObject, lifeTime);
        }
    }
}