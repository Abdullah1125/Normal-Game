using UnityEngine;

public class GravityButtonTrigger : MonoBehaviour
{
    public float customForce = 9.81f;

    public void ExecuteFlip()
    {
        float currentDirection = Mathf.Sign(Physics2D.gravity.y);
        float newDirection = -currentDirection;
        Physics2D.gravity = new Vector2(0, newDirection * customForce);
        Debug.Log("Buton yer çekimini çevirdi.");

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateGravityDirection();
        }
    }
}