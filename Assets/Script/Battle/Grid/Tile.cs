using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile Data")]
    public TileData tileData;

    [Header("Tile Colors")]
    [SerializeField] private Color _baseColor = Color.white;
    [SerializeField] private Color _offsetColor = Color.gray;

    [Header("Hover & Move/Attack Objects")]
    [SerializeField] private GameObject _hoverObject;
    [SerializeField] private GameObject _moveAreaObject;
    [SerializeField] private GameObject _attackAreaObject; // 🟩 NEW

    [HideInInspector] public int moveCost;
    [HideInInspector] public bool isMoveArea;
    [HideInInspector] public bool isAttackArea; // 🟩 NEW

    private Renderer _renderer;
    private Material _materialInstance;
    private Color _originalColor;

    [HideInInspector] public int gridX;
    [HideInInspector] public int gridZ;

    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public EntityMaster occupyingEntity;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materialInstance = new Material(_renderer.sharedMaterial);
        _renderer.material = _materialInstance;

        moveCost = tileData.moveCost;
        isMoveArea = tileData.isMoveArea;

        _hoverObject?.SetActive(false);
        _moveAreaObject?.SetActive(false);
        _attackAreaObject?.SetActive(false); // 🟩 NEW
    }

    public void Init(bool isOffset)
    {
        _originalColor = isOffset ? _offsetColor : _baseColor;
        _materialInstance.color = _originalColor;
    }

    private void OnMouseEnter()
    {
        if (_hoverObject != null)
            _hoverObject.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (_hoverObject != null)
            _hoverObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Faction currentTurn = TurnManager.GetCurrentTurn();
        if (currentTurn != Faction.PLAYER) return;

        // 🔹 Case 1: Clicked Player unit
        if (isOccupied && occupyingEntity != null)
        {
            if (occupyingEntity.Faction == Faction.PLAYER && !occupyingEntity.movementManager.HasMoved)
            {
                var playerManager = PlayerManager.Instance;
                playerManager.ClearAllMoveAreas();

                playerManager.SetSelectedEntity(occupyingEntity);
                ShowMoveAreaBFS(occupyingEntity.movementManager.MoveRange);

                // 🟩 Also show attack range immediately
                ShowAttackAreaBFS(occupyingEntity.attackManager.AttackRange);

                return;
            }
        }

        if (isMoveArea && PlayerManager.Instance.SelectedEntity != null)
        {
            var entity = PlayerManager.Instance.SelectedEntity;
            // Only call PlayerManager.TileClicked to trigger movement coroutine
            PlayerManager.Instance.TileClicked(this); // Or send to MoveAndEnableAttack
            return;
        }

        // 🔹 Case 3: Attack a target
        if (isAttackArea && PlayerManager.Instance.SelectedEntity != null)
        {
            var attacker = PlayerManager.Instance.SelectedEntity;

            if (isOccupied && occupyingEntity != null && occupyingEntity.Faction != attacker.Faction)
            {
                Debug.Log($"[Tile] {attacker.name} attacks {occupyingEntity.name}!");
                attacker.attackManager.Attack(occupyingEntity);
            }

            PlayerManager.Instance.ClearAllMoveAreas();
            PlayerManager.Instance.SetSelectedEntity(null);
            return;
        }

        PlayerManager.Instance.ClearAllMoveAreas();
    }

    // 🔹 Show movement range (unchanged)
    public void ShowMoveAreaBFS(int moveRange)
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((this, moveRange));
        visited.Add(this);

        while (queue.Count > 0)
        {
            var (currentTile, rangeLeft) = queue.Dequeue();
            currentTile.ActivateMoveAreaObject();
            if (rangeLeft <= 0) continue;

            Vector2Int coords = grid.GetTileCoordinates(currentTile);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Tile neighbor = grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null || visited.Contains(neighbor) || neighbor.isOccupied) continue;

                int newRange = rangeLeft - neighbor.moveCost;
                if (newRange < 0) continue;

                queue.Enqueue((neighbor, newRange));
                visited.Add(neighbor);
            }
        }
    }

    public void ShowAttackAreaBFS(int attackRange)
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((this, attackRange));
        visited.Add(this);

        var attackerFaction = PlayerManager.Instance.SelectedEntity?.Faction;

        while (queue.Count > 0)
        {
            var (currentTile, rangeLeft) = queue.Dequeue();

            if (currentTile != this)
            {
                // Only show attack if tile has an enemy
                if (currentTile.isOccupied && currentTile.occupyingEntity.Faction != attackerFaction)
                    currentTile.ActivateAttackAreaObject();
            }

            if (rangeLeft <= 0)
                continue;

            Vector2Int coords = grid.GetTileCoordinates(currentTile);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Tile neighbor = grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                // Only propagate through walkable tiles
                if (!neighbor.tileData.isMoveArea)
                {
                    // Show attack only if there is an enemy
                    if (neighbor.isOccupied && neighbor.occupyingEntity.Faction != attackerFaction)
                        neighbor.ActivateAttackAreaObject();

                    visited.Add(neighbor);
                    continue;
                }

                // ✅ Normal propagation
                queue.Enqueue((neighbor, rangeLeft - 1));
                visited.Add(neighbor);
            }
        }
    }

    public void ActivateMoveAreaObject()
    {
        isMoveArea = true;
        _moveAreaObject?.SetActive(true);
    }
    // 🟩 NEW
    public void ActivateAttackAreaObject()
    {
        isAttackArea = true;
        _attackAreaObject?.SetActive(true);
    }

    public void SetOccupyingEntity(EntityMaster entity)
    {
        occupyingEntity = entity;
        isOccupied = entity != null;
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    public Vector3 GetCenterPosition(float heightOffset = 0f)
    {
        Vector3 pos = transform.position;
        return new Vector3(pos.x, pos.y + heightOffset, pos.z);
    }

    public EntityMaster GetOccupyingEntity() => occupyingEntity;


    public void ClearAttackArea()
    {
        isAttackArea = false;
        _attackAreaObject?.SetActive(false);
    }
    public void ClearMoveArea()
    {
        isMoveArea = false;
        _moveAreaObject?.SetActive(false);
        isAttackArea = false; // 🟩 NEW
        _attackAreaObject?.SetActive(false); // 🟩 NEW
    }
}
