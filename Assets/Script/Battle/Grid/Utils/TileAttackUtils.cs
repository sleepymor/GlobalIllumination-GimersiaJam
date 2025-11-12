using UnityEngine;
using System.Collections.Generic;

public class TileAttackUtils
{
    private Tile _t;

    private static readonly List<Tile> _previousAOETiles = new List<Tile>();

    public TileAttackUtils(Tile tile)
    {
        _t = tile;
    }

    public void ActivateAttackAreaObject()
    {
        _t.isAttackArea = true;
        _t._attackAreaObject?.SetActive(true);
    }

    public void ClearAttackArea()
    {
        _t.isAttackArea = false;
        _t._attackAreaObject?.SetActive(false);
    }

    // ======================================================
    // 🔹 ATTACK AREA
    // ======================================================
    public void ShowAttackAreaBFS(int attackRange)
    {
        if (_t.grid == null) return;

        Queue<(Tile tile, int remainingRange)> queue = new();
        HashSet<Tile> visited = new();

        queue.Enqueue((_t, attackRange));
        visited.Add(_t);

        var attackerFaction = PlayerManager.Instance.SelectedEntity?.data.faction;

        while (queue.Count > 0)
        {
            var (currentTile, rangeLeft) = queue.Dequeue();

            if (currentTile != _t)
            {
                if (currentTile.isOccupied && currentTile.occupyingEntity.data.faction != attackerFaction)
                    currentTile.tileAttack.ActivateAttackAreaObject();
            }

            if (rangeLeft <= 0)
                continue;

            Vector2Int coords = _t.grid.GetTileCoordinates(currentTile);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Tile neighbor = _t.grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                if (!neighbor.tileData.isMoveArea)
                {
                    if (neighbor.isOccupied && neighbor.occupyingEntity.data.faction != attackerFaction)
                        neighbor.tileAttack.ActivateAttackAreaObject();

                    visited.Add(neighbor);
                    continue;
                }

                queue.Enqueue((neighbor, rangeLeft - 1));
                visited.Add(neighbor);
            }
        }
    }

    // ======================================================
    // 🔹 AOE AREA
    // ======================================================
    public void ShowAOEAreaBFS(int attackRange)
    {
        if (_t.grid == null) return;

        ClearAOEArea();

        Queue<(Tile tile, int remainingRange)> queue = new();
        HashSet<Tile> visited = new();

        queue.Enqueue((_t, attackRange));
        visited.Add(_t);

        while (queue.Count > 0)
        {
            var (currentTile, rangeLeft) = queue.Dequeue();

            // 🔹 Aktifkan area dan ubah warna
            currentTile.tileAction.ActivateActionAreaObject();
            _previousAOETiles.Add(currentTile);

            var sr = currentTile.tileAction._t.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.red; // warna AOE

            if (rangeLeft <= 0) continue;

            Vector2Int coords = _t.grid.GetTileCoordinates(currentTile);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Tile neighbor = _t.grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null || visited.Contains(neighbor))
                    continue;

                queue.Enqueue((neighbor, rangeLeft - 1));
                visited.Add(neighbor);
            }
        }
    }

    public void ClearAOEArea()
    {
        foreach (var tile in _previousAOETiles)
        {
            tile.tileAction.ClearActionArea();

            // // 🔹 Kembalikan ke warna asli sprite (Color.white = no tint)
            // var sr = tile.tileAction._t.GetComponent<SpriteRenderer>();
            // if (sr != null)
            //     sr.color = Color.white;
        }

        _previousAOETiles.Clear();
    }

    // ======================================================
    // 🔹 DAMAGE LOGIC
    // ======================================================

    /// <summary>
    /// Deals damage to a specific tile's occupying entity.
    /// </summary>
    public void DealDamage(Tile targetTile, int amount)
    {
        if (targetTile == null || !targetTile.isOccupied) return;

        var entity = targetTile.occupyingEntity;
        if (entity == null) return;

        entity.health.TakeDamage(amount);

        Debug.Log($"[{_t.name}] dealt {amount} damage to {entity.name}!");
    }

    /// <summary>
    /// Deals AOE damage around this tile using BFS range.
    /// </summary>
    public void DealAOEDamage(int attackRange, int amount)
    {
        if (_t.grid == null) return;

        Queue<(Tile tile, int remainingRange)> queue = new();
        HashSet<Tile> visited = new();

        queue.Enqueue((_t, attackRange));
        visited.Add(_t);

        // 🔸 Tidak ada pengecekan faction lagi

        while (queue.Count > 0)
        {
            var (current, rangeLeft) = queue.Dequeue();

            // 🔹 Tile asal sekarang juga bisa kena damage
            if (current.isOccupied)
            {
                var entity = current.occupyingEntity;
                if (entity != null)
                {
                    entity.health.TakeDamage(amount);
                    Debug.Log($"[AOE] {entity.name} di tile {current.name} menerima {amount} damage!");
                }
            }

            if (rangeLeft <= 0) continue;

            Vector2Int coords = _t.grid.GetTileCoordinates(current);
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Tile neighbor = _t.grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                if (neighbor == null || visited.Contains(neighbor)) continue;

                queue.Enqueue((neighbor, rangeLeft - 1));
                visited.Add(neighbor);
            }
        }
    }

}
