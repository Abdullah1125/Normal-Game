using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Oyuncunun bastığı tile'ları dökerek yıkar. 
/// Object Pooling kullanarak performans kaybını önler.
/// </summary>
[RequireComponent(typeof(Tilemap))]
public class VerticalCascadeCrumbler : MonoBehaviour, IResettable
{
    [Header("Cascade Settings (Şelale Ayarları)")]
    public GameObject fallingGhostPrefab;
    public float cascadeDelay = 0.04f;
    public float destroyDelay = 1.5f;

    [Header("Pool Settings (Havuz Ayarları)")]
    public int poolSize = 100;
    private Queue<GameObject> _ghostPool = new Queue<GameObject>();
    private List<GameObject> _activePoolGhosts = new List<GameObject>();

    [Header("Player Tracking (Oyuncu Takibi)")]
    public float movementThreshold = 0.1f;
    public float footSampleOffset = 0.05f;

    private Tilemap _tilemap;
    private Rigidbody2D _playerRigidbody;
    private Collider2D _playerCollider;
    private GameObject _playerObj;
    private GameObject _normalGridObj;

    [Header("State Tracking (Durum Takibi)")]
    private HashSet<int> _activeCascadeColumns = new HashSet<int>();
    private Dictionary<Vector3Int, TileBase> _destroyedTilesCache = new Dictionary<Vector3Int, TileBase>();
    private Vector3Int? _currentPlayerTilePos = null;
    private int? _lastCrumbledColumn = null;

    void Awake()
    {
        _tilemap = GetComponent<Tilemap>();

        if (PlayerController.Instance != null)
        {
            _playerObj = PlayerController.Instance.gameObject;
            _playerRigidbody = PlayerController.Instance.GetComponent<Rigidbody2D>();
            _playerCollider = PlayerController.Instance.GetComponent<Collider2D>();
        }

        _normalGridObj = GameObject.FindGameObjectWithTag(Constants.TAG_TARGET_GRID);

        // Havuz başlangıç ataması
        InitializePool();
    }

    /// <summary>
    /// Sahne başında belirlenen miktarda objeyi oluşturup pasif yapar.
    /// </summary>
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ghost = Instantiate(fallingGhostPrefab);
            ghost.SetActive(false);
            _ghostPool.Enqueue(ghost);
        }
    }

    void Start()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.RegisterResettable(this);

        if (_normalGridObj != null)
            _normalGridObj.SetActive(false);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (_playerObj == null || collision.gameObject != _playerObj || _playerRigidbody == null || _playerCollider == null)
            return;

        float horizontalVelocity = Mathf.Abs(_playerRigidbody.linearVelocity.x);

        if (horizontalVelocity <= movementThreshold)
            return;

        if (!TryGetPlayerGroundCell(out Vector3Int cellPos))
            return;

        _currentPlayerTilePos = cellPos;
        int targetX = cellPos.x;
        int targetY = cellPos.y;

        if (_lastCrumbledColumn.HasValue)
        {
            int startX = _lastCrumbledColumn.Value;
            if (startX == targetX)
            {
                TryStartCascadeAtColumn(targetX, targetY);
            }
            else
            {
                int step = (targetX > startX) ? 1 : -1;
                for (int x = startX + step; x != targetX + step; x += step)
                {
                    TryStartCascadeAtColumn(x, targetY);
                }
            }
        }
        else
        {
            TryStartCascadeAtColumn(targetX, targetY);
        }

        _lastCrumbledColumn = targetX;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (_playerObj != null && collision.gameObject == _playerObj)
        {
            _currentPlayerTilePos = null;
            _lastCrumbledColumn = null;
        }
    }

    private bool TryGetPlayerGroundCell(out Vector3Int cellPos)
    {
        cellPos = default;
        if (_playerCollider == null) return false;

        Bounds b = _playerCollider.bounds;
        Vector3 sampleCenter = new Vector3(b.center.x, b.min.y - footSampleOffset, 0f);
        Vector3Int centerCell = _tilemap.WorldToCell(sampleCenter);

        if (_tilemap.HasTile(centerCell))
        {
            cellPos = centerCell;
            return true;
        }

        float xOffset = b.extents.x * 0.7f;
        Vector3 sampleLeft = new Vector3(b.center.x - xOffset, b.min.y - footSampleOffset, 0f);
        Vector3Int leftCell = _tilemap.WorldToCell(sampleLeft);

        if (_tilemap.HasTile(leftCell))
        {
            cellPos = leftCell;
            return true;
        }

        Vector3 sampleRight = new Vector3(b.center.x + xOffset, b.min.y - footSampleOffset, 0f);
        Vector3Int rightCell = _tilemap.WorldToCell(sampleRight);

        if (_tilemap.HasTile(rightCell))
        {
            cellPos = rightCell;
            return true;
        }

        return false;
    }

    private bool TryFindTileInColumn(int x, int aroundY, out int foundY)
    {
        for (int y = aroundY + 1; y >= aroundY - 2; y--)
        {
            if (_tilemap.HasTile(new Vector3Int(x, y, 0)))
            {
                foundY = y;
                return true;
            }
        }
        foundY = 0;
        return false;
    }

    private void TryStartCascadeAtColumn(int columnX, int aroundY)
    {
        if (_activeCascadeColumns.Contains(columnX)) return;

        if (TryFindTileInColumn(columnX, aroundY, out int foundY))
        {
            StartCoroutine(VerticalCascadeRoutine(columnX, foundY));
        }
    }

    private IEnumerator VerticalCascadeRoutine(int columnX, int startY)
    {
        _activeCascadeColumns.Add(columnX);
        int currentY = startY;

        while (_tilemap.HasTile(new Vector3Int(columnX, currentY, 0)))
        {
            Vector3Int currentCell = new Vector3Int(columnX, currentY, 0);
            ReplaceTileWithGhost(currentCell);

            yield return new WaitForSeconds(cascadeDelay);
            currentY--;

            if (currentY < -500) break;
        }
        _activeCascadeColumns.Remove(columnX);
    }

    private void ReplaceTileWithGhost(Vector3Int cellPos)
    {
        TileBase originalTile = _tilemap.GetTile(cellPos);
        Sprite tileSprite = _tilemap.GetSprite(cellPos);

        if (originalTile == null || tileSprite == null) return;

        if (!_destroyedTilesCache.ContainsKey(cellPos))
            _destroyedTilesCache.Add(cellPos, originalTile);

        Vector3 worldPos = _tilemap.GetCellCenterWorld(cellPos);
        _tilemap.SetTile(cellPos, null);

        // Havuzdan obje çek
        if (_ghostPool.Count > 0)
        {
            GameObject ghost = _ghostPool.Dequeue();
            ghost.transform.position = worldPos;

            SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = tileSprite;

            ghost.SetActive(true);
            _activePoolGhosts.Add(ghost);
            StartCoroutine(ReturnGhostToPool(ghost, destroyDelay));
        }
    }

    private IEnumerator ReturnGhostToPool(GameObject ghost, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ghost.activeSelf)
        {
            ghost.SetActive(false);
            _activePoolGhosts.Remove(ghost);
            _ghostPool.Enqueue(ghost);
        }
    }

    public void ResetMechanic()
    {
        StopAllCoroutines();

        // Aktif tüm havuz objelerini kapat ve geri topla
        foreach (GameObject ghost in _activePoolGhosts.ToArray())
        {
            ghost.SetActive(false);
            _ghostPool.Enqueue(ghost);
        }
        _activePoolGhosts.Clear();

        _activeCascadeColumns.Clear();
        _currentPlayerTilePos = null;
        _lastCrumbledColumn = null;

        foreach (KeyValuePair<Vector3Int, TileBase> kvp in _destroyedTilesCache)
        {
            _tilemap.SetTile(kvp.Key, kvp.Value);
        }
        _destroyedTilesCache.Clear();
    }

    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.UnregisterResettable(this);

        if (_normalGridObj != null)
            _normalGridObj.SetActive(true);

        // Temizlik
        foreach (GameObject g in _ghostPool) if (g != null) Destroy(g);
        foreach (GameObject g in _activePoolGhosts) if (g != null) Destroy(g);
    }
}