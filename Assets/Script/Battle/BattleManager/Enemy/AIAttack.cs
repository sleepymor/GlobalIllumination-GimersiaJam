using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AIAttack
{
    EnemyManager _m;
    GridManager grid;
    public AIAttack(EnemyManager m)
    {
        _m = m;
        grid = _m.grid;
    }

    public Tile FindClosestTileTowardsTarget(EntityMaster self, EntityMaster target, GridManager grid)
    {
        if (grid == null) return null;

        Tile startTile = grid.GetTileAt(self.pos.GridX, self.pos.GridZ);
        Tile targetTile = grid.GetTileAt(target.pos.GridX, target.pos.GridZ);
        if (startTile == null || targetTile == null) return null;

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();
        List<Tile> reachable = new List<Tile>();

        queue.Enqueue((startTile, self.move.MoveRange));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (current, rangeLeft) = queue.Dequeue();
            if (rangeLeft <= 0) continue;

            foreach (Tile neighbor in grid.GetNeighbors(current))
            {
                if (neighbor == null || visited.Contains(neighbor) || neighbor.isOccupied) continue;

                int newRange = rangeLeft - neighbor.tileMove.moveCost;
                if (newRange < 0) continue;

                visited.Add(neighbor);
                queue.Enqueue((neighbor, newRange));
                reachable.Add(neighbor);
            }
        }

        // === Tambahan logika untuk ranged enemy ===
        int attackRange = self.data.attackRange;
        bool isRanged = attackRange > 2;

        if (isRanged)
        {
            // Cari tile terdekat yang masih di dalam jarak tembak
            var inRangeTiles = reachable
                .Where(t =>
                    Vector3.Distance(t.transform.position, targetTile.transform.position) <= attackRange
                )
                .OrderBy(t => Vector3.Distance(t.transform.position, targetTile.transform.position))
                .ToList();

            if (inRangeTiles.Count > 0)
            {
                // Jika sudah ada tile dalam jarak serang, ambil yang paling dekat ke target
                return inRangeTiles.First();
            }
            else
            {
                // Kalau belum bisa nyerang, bergerak mendekat tapi tetap jaga jarak
                return reachable
                    .OrderBy(t => Vector3.Distance(t.transform.position, targetTile.transform.position))
                    .FirstOrDefault();
            }
        }
        else
        {
            // === Untuk melee enemy (attackRange <= 2) ===
            return reachable
                .OrderBy(t => Vector3.Distance(t.transform.position, targetTile.transform.position))
                .FirstOrDefault();
        }
    }

    public void TryAttackTarget(EntityMaster attacker, EntityMaster target)
    {
        if (attacker.attack.CanAttack(target))
        {
            attacker.attack.Attack(target);
            attacker.attack.SetHadAttacking(true);
            Debug.Log($"[EnemyManager] {attacker.name} attacked {target.name}");
        }
        else
        {
            Debug.Log($"[EnemyManager] {attacker.name} could not attack {target.name} (out of range)");
        }
    }

}