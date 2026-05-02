using UnityEngine;

/// <summary>
/// Highly optimized gravity flipper using Physics triggers instead of Update checks.
/// (Update kontrolleri yerine fizik tetikleyicileri kullanan, yüksek düzeyde optimize edilmiþ yer çekimi deðiþtirici.)
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class GravityGate : MonoBehaviour, IResettable
{
    [Header("Settings (Ayarlar)")]
    public float gravityForce = 9.81f;

    [Tooltip("If true, crossing from left to right sets gravity UP. (Soldan saða geçiþ yer çekimini YUKARI ayarlar.)")]
    public bool invertLogic = false;

    private BoxCollider2D _gateCollider;
    private float _initialGravityY;

    /// <summary>
    /// Configures the trigger and saves initial gravity state.
    /// (Tetikleyiciyi yapýlandýrýr ve baþlangýç yer çekimi durumunu kaydeder.)
    /// </summary>
    private void Awake()
    {
        _gateCollider = GetComponent<BoxCollider2D>();
        _gateCollider.isTrigger = true;
        _initialGravityY = Physics2D.gravity.y;
    }

    private void Start()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterResettable(this);
        }
    }

    /// <summary>
    /// Detects player passing through and flips gravity based on exit direction.
    /// (Oyuncunun geçiþini algýlar ve çýkýþ yönüne göre yer çekimini deðiþtirir.)
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            // Karakterin merkeze göre hangi tarafta kaldýðýný belirle
            bool isRightSide = other.transform.position.x > transform.position.x;

            // Lojik: Saðdayken yukarý, soldayken aþaðý (Veya tam tersi)
            float direction = isRightSide ? 1f : -1f;
            if (invertLogic) direction *= -1;

            ApplyGravity(direction);
        }
    }

    private void ApplyGravity(float direction)
    {
        Physics2D.gravity = new Vector2(0, direction * gravityForce);

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateGravityDirection();
        }
    }

    /// <summary>
    /// Resets the global gravity to the level's default state.
    /// (Global yer çekimini bölümün varsayýlan durumuna sýfýrlar.)
    /// </summary>
    public void ResetMechanic()
    {
        Physics2D.gravity = new Vector2(0, _initialGravityY);
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.UpdateGravityDirection();
        }
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnregisterResettable(this);
        }
    }
}