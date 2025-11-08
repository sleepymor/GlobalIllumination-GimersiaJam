using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;


[RequireComponent(typeof(Renderer), typeof(Collider))]
public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile Colors")]
    [SerializeField] private Color _baseColor = Color.white;
    [SerializeField] private Color _offsetColor = Color.gray;

    [Header("Hover & Move Area Objects")]
    [SerializeField] private GameObject _hoverObject;
    [SerializeField] private GameObject _moveAreaObject;

    private Renderer _renderer;
    private Material _materialInstance;
    private Color _originalColor;

    [HideInInspector] public int gridX;
    [HideInInspector] public int gridZ;
    public bool _isMoveArea = false;

    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public int moveCost = 2;
    public EntityMaster occupyingEntity;

    
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materialInstance = new Material(_renderer.sharedMaterial);
        _renderer.material = _materialInstance;

        _hoverObject?.SetActive(false);
        if (_moveAreaObject != null) _moveAreaObject.SetActive(false);
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
        // Case 1: Tile has an entity (occupied) and it’s a player unit
        if (isOccupied && occupyingEntity != null)
        {
            if (occupyingEntity.Faction == Faction.PLAYER && !occupyingEntity.HasMoved)
            {
                TurnManager.SelectedEntity = occupyingEntity;
                TurnManager.Instance.ClearAllMoveAreas();
                ShowMoveAreaBFS(occupyingEntity.MoveRange);
                return;
            }
        }

        // Case 2: Tile is a move-area tile and a player entity is selected
        if (_isMoveArea && TurnManager.SelectedEntity != null)
        {
            TurnManager.SelectedEntity.StartCoroutine(
                TurnManager.SelectedEntity.MoveToGridPosition(gridX, gridZ)
            );
            TurnManager.Instance.ClearAllMoveAreas();
            TurnManager.SelectedEntity = null;
            return;
        }

        TurnManager.Instance.ClearAllMoveAreas();
    }

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
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in directions)
            {
                Tile neighbor = grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null) continue;
                if (visited.Contains(neighbor)) continue;

                if (neighbor.isOccupied) continue;

                int moveCost = neighbor.moveCost; 
                int newRange = rangeLeft - moveCost;
                if (newRange < 0) continue;

                queue.Enqueue((neighbor, newRange));
                visited.Add(neighbor);
            }
        }
    }

    public void ActivateMoveAreaObject()
    {
        _isMoveArea = true;
        _moveAreaObject?.SetActive(true);
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

    public void ClearMoveArea()
    {
        _isMoveArea = false;
        _moveAreaObject?.SetActive(false);
    }
}
