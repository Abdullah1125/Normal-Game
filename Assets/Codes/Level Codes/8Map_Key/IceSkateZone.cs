using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class IceSkateZone : MonoBehaviour, IResettable
{
    [Header("Buz Pateni Ayarları")]
    public float maxSlideSpeed = 5f;
    public float acceleration = 8f;

    [Header("Sadece Aşağı Yön Butonu (İsteğe Bağlı)")]
    public GameObject downButton;

    private Rigidbody2D _playerRb;
    private Animator _playerAnim;

    private float _origGravityScale;
    private bool _isActive = false;

    private Vector2 _currentSlideDirection = Vector2.zero;
    private bool _isSliding = false;
    private float _currentSpeed = 0f;

    private IceSkateCollisionHelper _collisionHelper;
    private FieldInfo _leftBtnField;
    private FieldInfo _rightBtnField;
    private FieldInfo _isHoldingJumpField;

    private List<GameObject> _disabledObstacles = new List<GameObject>();
    private List<Collider2D> _obstacleColliders = new List<Collider2D>();
    private Collider2D[] _playerColliders;

    public bool IsSliding => _isSliding;
    public Vector2 CurrentDirection => _currentSlideDirection;

    void Start()
    {
        if (PlayerController.Instance == null) return;
        _playerRb = PlayerController.Instance.GetComponent<Rigidbody2D>();
        _playerAnim = PlayerController.Instance.GetComponent<Animator>();

        if (_playerRb != null) _origGravityScale = _playerRb.gravityScale;

        _leftBtnField = typeof(MobileDirectionButton).GetField("leftPressed", BindingFlags.NonPublic | BindingFlags.Static);
        _rightBtnField = typeof(MobileDirectionButton).GetField("rightPressed", BindingFlags.NonPublic | BindingFlags.Static);
        _isHoldingJumpField = typeof(PlayerController).GetField("isHoldingJump", BindingFlags.NonPublic | BindingFlags.Instance);

        if (downButton != null) downButton.SetActive(false);

        Apply();
    }

    private void Apply()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = true;
            PlayerController.Instance.enabled = false;

            _playerColliders = PlayerController.Instance.GetComponentsInChildren<Collider2D>();

            _collisionHelper = PlayerController.Instance.gameObject.AddComponent<IceSkateCollisionHelper>();
            _collisionHelper.Init(this);
        }

        if (_playerRb != null)
        {
            _playerRb.gravityScale = 0f;
            _playerRb.linearVelocity = Vector2.zero;
        }

        _disabledObstacles.Clear();
        _obstacleColliders.Clear();

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var obs in obstacles)
        {
            obs.tag = "Untagged";
            _disabledObstacles.Add(obs);

            Collider2D[] cols = obs.GetComponentsInChildren<Collider2D>();
            foreach (var oCol in cols)
            {
                _obstacleColliders.Add(oCol);
                if (_playerColliders != null)
                {
                    foreach (var pCol in _playerColliders)
                    {
                        Physics2D.IgnoreCollision(pCol, oCol, true);
                    }
                }
            }
        }

        if (downButton != null) downButton.SetActive(true);

        _isSliding = false;
        _currentSlideDirection = Vector2.zero;
        _currentSpeed = 0f;
        _isActive = true;
    }

    void Update()
    {
        if (!_isActive || _playerRb == null) return;

        HandleInputs();
        CheckObstacleVisuals();

        if (_playerAnim != null)
        {
            _playerAnim.SetBool("isWalking", _isSliding);
            _playerAnim.SetBool("isGrounded", true);
        }
    }

    private void HandleInputs()
    {
        if (!_isSliding)
        {
            float hor = Input.GetAxisRaw("Horizontal");
            float ver = Input.GetAxisRaw("Vertical");

            if (_leftBtnField != null && _rightBtnField != null)
            {
                bool leftPressed = (bool)_leftBtnField.GetValue(null);
                bool rightPressed = (bool)_rightBtnField.GetValue(null);

                if (leftPressed) hor = -1f;
                else if (rightPressed) hor = 1f;
            }

            if (_isHoldingJumpField != null)
            {
                bool jumpPressed = (bool)_isHoldingJumpField.GetValue(PlayerController.Instance);
                if (jumpPressed) ver = 1f;
            }

            if (hor != 0)
            {
                TrySlide(new Vector2(hor, 0).normalized);
                UpdateSpriteDirection(hor);
            }
            else if (ver != 0)
            {
                TrySlide(new Vector2(0, ver).normalized);
            }
        }
    }

    private void CheckObstacleVisuals()
    {
        if (_playerColliders == null || _playerColliders.Length == 0) return;

        foreach (var obs in _disabledObstacles)
        {
            if (obs == null) continue;

            bool isInside = false;
            Collider2D[] obsCols = obs.GetComponentsInChildren<Collider2D>();

            foreach (var oCol in obsCols)
            {
                foreach (var pCol in _playerColliders)
                {
                    if (pCol != null && pCol.bounds.Intersects(oCol.bounds))
                    {
                        isInside = true;
                        break;
                    }
                }
                if (isInside) break;
            }

            SpriteRenderer[] srs = obs.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in srs)
            {
                if (sr.enabled == isInside)
                {
                    sr.enabled = !isInside;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!_isActive || _playerRb == null) return;

        if (_isSliding)
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, maxSlideSpeed, acceleration * Time.fixedDeltaTime);
            _playerRb.linearVelocity = _currentSlideDirection * _currentSpeed;
        }
        else
        {
            _playerRb.linearVelocity = Vector2.zero;
        }
    }

    public void MoveDown() { TrySlide(Vector2.down); }
    public void MoveUp() { TrySlide(Vector2.up); }
    public void MoveLeft() { TrySlide(Vector2.left); UpdateSpriteDirection(-1); }
    public void MoveRight() { TrySlide(Vector2.right); UpdateSpriteDirection(1); }

    private void TrySlide(Vector2 dir)
    {
        if (!_isActive || _playerRb == null || _isSliding) return;

        _currentSlideDirection = dir;
        _currentSpeed = 0f;
        _isSliding = true;
    }

    public void OnWallHit()
    {
        if (_isSliding)
        {
            _isSliding = false;
            _currentSlideDirection = Vector2.zero;
            _currentSpeed = 0f;

            if (_playerRb != null) _playerRb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateSpriteDirection(float hor)
    {
        SpriteRenderer sr = PlayerController.Instance.GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = hor < 0;
    }

    public void ResetMechanic()
    {
        Restore();
        Apply();
    }

    private void OnDestroy()
    {
        Restore();
    }

    private void Restore()
    {
        if (!_isActive) return;

        if (_playerRb != null) _playerRb.gravityScale = _origGravityScale;

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.canMove = true;
            PlayerController.Instance.enabled = true;
        }

        foreach (var obs in _disabledObstacles)
        {
            if (obs != null)
            {
                obs.tag = "Obstacle";
                SpriteRenderer[] srs = obs.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in srs) sr.enabled = true;
            }
        }

        foreach (var oCol in _obstacleColliders)
        {
            if (oCol != null && _playerColliders != null)
            {
                foreach (var pCol in _playerColliders)
                {
                    if (pCol != null)
                    {
                        Physics2D.IgnoreCollision(pCol, oCol, false);
                    }
                }
            }
        }

        _disabledObstacles.Clear();
        _obstacleColliders.Clear();

        if (_collisionHelper != null) Destroy(_collisionHelper);

        if (downButton != null) downButton.SetActive(false);

        _isActive = false;
    }
}

public class IceSkateCollisionHelper : MonoBehaviour
{
    private IceSkateZone _zoneController;
    public void Init(IceSkateZone controller) { _zoneController = controller; }

    private void OnCollisionEnter2D(Collision2D other) { CheckCollision(other); }
    private void OnCollisionStay2D(Collision2D other) { CheckCollision(other); }

    private void CheckCollision(Collision2D other)
    {
        if (_zoneController == null || !_zoneController.IsSliding) return;

        foreach (ContactPoint2D contact in other.contacts)
        {
            float dot = Vector2.Dot(contact.normal, _zoneController.CurrentDirection);
            if (dot < -0.5f)
            {
                _zoneController.OnWallHit();
                break;
            }
        }
    }
}
