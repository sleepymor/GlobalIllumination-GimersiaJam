using UnityEngine;
using System.Collections.Generic;

public class TileActionUtils
{
    public Tile _t;
    public TileActionUtils(Tile tile)
    {
        _t = tile;
    }

    public void ActivateActionAreaObject()
    {
        if (_t._actionAreaObject == null) return;
        _t._actionAreaObject.SetActive(true);
        _t.isActionArea = true;
    }

    public void ShowActionAreaBFS(int summonRange, bool isEquip = false)
    {
        if (_t.grid == null) return;

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((_t, summonRange));
        visited.Add(_t);

        while (queue.Count > 0)
        {
            var (currentTile, rangeLeft) = queue.Dequeue();

            if (!currentTile.isOccupied || isEquip) currentTile.tileAction.ActivateActionAreaObject();
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

    public void ClearActionArea()
    {
        if (_t._actionAreaObject == null) return;
        _t._actionAreaObject.SetActive(false);
        _t.isActionArea = false;
    }
}