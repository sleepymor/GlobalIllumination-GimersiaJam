/*
 * EntityMovement.cs
 * -----------------
 * Handles the movement logic for an EntityMaster in a grid-based, turn-based strategy game.
 *
 * Responsibilities:
 * - Track movement state (isMoving, hasMoved) for the current turn.
 * - Provide movement range based on the entity's data.
 * - Snap the entity instantly to a grid tile or move smoothly via animation.
 * - Perform pathfinding (BFS with cost tracking) to navigate from current tile to a target tile.
 * - Update tile occupancy when moving.
 * - Interact with the EntityMaster and GridManager to maintain proper position and state.
 *
 * Usage:
 * - Attach this script to the same GameObject as EntityMaster.
 * - Initialize with EntityMaster.Initialize(thisEntity) after Awake.
 * - Call SnapToGridPosition(x, z) to teleport the entity.
 * - Call MoveToGridPosition(x, z) as a coroutine to move smoothly across tiles.
 * - Use SetHadMove(bool) to manually reset the movement state if needed.
 *
 * Notes:
 * - Movement stops if the entity is dead or already moving.
 * - The BFS pathfinding ensures the entity does not exceed its movement range or move into occupied tiles.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EntityMovement
{

    private bool isMoving = false;
    private bool hasMoved = false;

    [HideInInspector] public int MoveRange;
    public bool HasMoved => hasMoved;

    private EntityMaster _e;

    public EntityMovement(EntityMaster e)
    {
        _e = e;
        MoveRange = _e.data.moveRange;
    }
    public void SnapToGridPosition(int x, int z)
    {
        if (_e.gridManager == null) return;

        Tile tile = _e.gridManager.GetTileAt(x, z);
        if (tile == null)
        {
            Debug.LogWarning($"[EntityMaster] Invalid grid position ({x},{z})!");
            return;
        }

        _e.currentTile?.SetOccupyingEntity(null);

        _e.currentTile = tile;
        _e.currentTile.SetOccupyingEntity(_e);

        _e.pos.SetPos(x, z);

        Vector3 tileCenter = tile.transform.position + Vector3.up * _e.pos.heightAboveTile;
        _e.transform.position = tileCenter;
    }

    public IEnumerator MoveToGridPosition(int targetX, int targetZ)
    {
        if (isMoving || _e.status.IsDead) yield break;
        if (_e.gridManager == null) yield break;

        List<Tile> path = FindPathTo(targetX, targetZ);
        if (path == null || path.Count == 0)
        {
            Debug.Log($"[EntityMaster] No valid path to ({targetX},{targetZ})");
            yield break;
        }

        isMoving = true;

        foreach (Tile stepTile in path)
        {
            if (stepTile == null) continue;
            yield return MoveStepAnim(stepTile);
        }

        isMoving = false;
        hasMoved = true;

        Debug.Log($"[EntityMaster] Finished moving to ({_e.pos.GridX},{_e.pos.GridZ})");
    }

    private IEnumerator MoveStepAnim(Tile targetTile)
    {
        if (targetTile == null) yield break;

        Vector3 startPos = _e.transform.position;
        Vector3 targetPos = targetTile.transform.position + Vector3.up * _e.pos.heightAboveTile;

        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / _e.anim.moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _e.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        _e.transform.position = targetPos;

        // Update occupancy
        _e.currentTile?.SetOccupyingEntity(null);
        _e.currentTile = targetTile;
        _e.currentTile.SetOccupyingEntity(_e);

        int x = _e.currentTile.gridX;
        int z = _e.currentTile.gridZ;
        _e.pos.SetPos(x, z);
    }


    private List<Tile> FindPathTo(int targetX, int targetZ)
    {
        if (_e.gridManager == null) return null;

        Tile start = _e.gridManager.GetTileAt(_e.pos.GridX, _e.pos.GridZ);
        Tile goal = _e.gridManager.GetTileAt(targetX, targetZ);
        if (start == null || goal == null)
        {
            Debug.LogWarning("[EntityMaster] Pathfinding failed: invalid start or goal tile!");
            return null;
        }

        // BFS with cost tracking
        Queue<(Tile tile, int costSoFar)> queue = new Queue<(Tile, int)>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((start, 0));
        visited.Add(start);

        while (queue.Count > 0)
        {
            var (current, costSoFar) = queue.Dequeue();
            if (current == goal)
                break;

            foreach (Tile neighbor in _e.gridManager.GetNeighbors(current))
            {
                if (neighbor == null || visited.Contains(neighbor) || neighbor.isOccupied)
                    continue;

                // ✅ NEW: Skip non-walkable tiles
                if (!neighbor.tileData.isMoveArea) // or neighbor.isWalkable if you use that
                    continue;

                int newCost = costSoFar + neighbor.moveCost;
                if (newCost > _e.data.moveRange)
                    continue;

                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                queue.Enqueue((neighbor, newCost));
            }

        }

        if (!cameFrom.ContainsKey(goal))
        {
            Debug.Log($"[EntityMaster] No path found to target ({targetX},{targetZ})");
            return null;
        }

        List<Tile> path = new List<Tile>();
        Tile step = goal;

        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }

        path.Reverse();
        return path;
    }


    public void SetHadMove(bool hasMoved)
    {
        this.hasMoved = hasMoved;
    }

}