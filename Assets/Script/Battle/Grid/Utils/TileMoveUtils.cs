using UnityEngine;
using System.Collections.Generic;

public class TileMoveUtils
{
    private Tile _t;
    public int moveCost;
    public TileMoveUtils(Tile tile)
    {
        _t = tile;
        moveCost = tile.tileData.moveCost;
    }

    public void ActivateMoveAreaObject()
    {
        _t.isMoveArea = true;
        _t._moveAreaObject?.SetActive(true);
    }

    public void ShowMoveAreaBFS(int moveRange)
    {
        if (_t.grid == null) return;

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((_t, moveRange));
        visited.Add(_t);

        while (queue.Count > 0)
        {
            var (currentTile, rangeLeft) = queue.Dequeue();
            currentTile.tileMove.ActivateMoveAreaObject();
            if (rangeLeft <= 0) continue;

            Vector2Int coords = _t.grid.GetTileCoordinates(currentTile);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Tile neighbor = _t.grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null || visited.Contains(neighbor) || neighbor.isOccupied) continue;

                int newRange = rangeLeft - neighbor.tileMove.moveCost;
                if (newRange < 0) continue;

                queue.Enqueue((neighbor, newRange));
                visited.Add(neighbor);
            }
        }
    }

    public void ClearMoveArea()
    {
        _t.isMoveArea = false;
        _t._moveAreaObject?.SetActive(false);
        _t.isAttackArea = false; 
        _t._attackAreaObject?.SetActive(false);
    }
}