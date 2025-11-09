using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : BattleEntityManager
{
    public static EnemyManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override Faction GetFactionType() => Faction.ENEMY;

    public IEnumerator RunEnemyTurn()
    {
        Debug.Log("<color=orange>[EnemyManager]</color> Enemy turn started...");

        RefreshTeam();

        if (TeamList.Count == 0)
        {
            Debug.LogWarning("[EnemyManager] No enemies to act. Ending turn early.");
            TurnManager.PlayerTurn();
            yield break;
        }

        foreach (var enemy in TeamList)
        {
            if (!IsEnemyValid(enemy)) continue;

            EntityMaster target = FindNearestTarget(enemy, PlayerManager.Instance.TeamList);
            if (target == null) continue;

            // Move enemy toward target
            Tile moveTile = FindClosestTileTowardsTarget(enemy, target);
            if (moveTile != null)
            {
                yield return enemy.StartCoroutine(
                    enemy.movementManager.MoveToGridPosition(moveTile.gridX, moveTile.gridZ)
                );
            }

            yield return new WaitForSeconds(0.2f); // small pause before attacking

            // Attack if in range
            TryAttackTarget(enemy, target);

            // Optional: small pause after attack
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("<color=orange>[EnemyManager]</color> Enemy turn ended.");
        TurnManager.PlayerTurn();
    }

    private bool IsEnemyValid(EntityMaster enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("[EnemyManager] Null enemy found — skipping.");
            return false;
        }

        if (enemy.deathManager.IsDead)
        {
            Debug.Log($"[EnemyManager] {enemy.name} is dead. Skipping.");
            return false;
        }

        if (enemy.movementManager.HasMoved)
        {
            Debug.Log($"[EnemyManager] {enemy.name} has already moved. Skipping.");
            return false;
        }

        return true;
    }

    private EntityMaster FindNearestTarget(EntityMaster self, List<EntityMaster> targets)
    {
        EntityMaster nearest = null;
        float bestDist = float.MaxValue;

        foreach (var t in targets)
        {
            if (t == null || t.deathManager.IsDead) continue;

            float dist = Vector3.Distance(self.transform.position, t.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = t;
            }
        }

        if (nearest != null)
            Debug.Log($"[EnemyManager] {self.name} selected nearest target: {nearest.name}");
        else
            Debug.LogWarning($"[EnemyManager] {self.name} found no alive player targets!");

        return nearest;
    }

    private Tile FindClosestTileTowardsTarget(EntityMaster self, EntityMaster target)
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return null;

        Tile startTile = grid.GetTileAt(self.GridX, self.GridZ);
        Tile targetTile = grid.GetTileAt(target.GridX, target.GridZ);
        if (startTile == null || targetTile == null) return null;

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();
        List<Tile> reachable = new List<Tile>();

        queue.Enqueue((startTile, self.movementManager.MoveRange));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (current, rangeLeft) = queue.Dequeue();
            if (rangeLeft <= 0) continue;

            foreach (Tile neighbor in grid.GetNeighbors(current))
            {
                if (neighbor == null || visited.Contains(neighbor) || neighbor.isOccupied) continue;

                int newRange = rangeLeft - neighbor.moveCost;
                if (newRange < 0) continue;

                visited.Add(neighbor);
                queue.Enqueue((neighbor, newRange));
                reachable.Add(neighbor);
            }
        }

        // Pick tile closest to target
        return reachable
            .OrderBy(t => Vector3.Distance(t.transform.position, targetTile.transform.position))
            .FirstOrDefault();
    }

    private void TryAttackTarget(EntityMaster attacker, EntityMaster target)
    {
        if (attacker.attackManager.CanAttack(target))
        {
            attacker.attackManager.Attack(target);
            attacker.attackManager.SetHadAttacking(true);
            Debug.Log($"[EnemyManager] {attacker.name} attacked {target.name}");
        }
        else
        {
            Debug.Log($"[EnemyManager] {attacker.name} could not attack {target.name} (out of range)");
        }
    }

    public override void EndTurn()
    {
        if (TurnManager.GetCurrentTurn() != Faction.ENEMY)
        {
            Debug.LogWarning("[EnemyManager] Tried to end turn, but it's not the enemy's turn!");
            return;
        }

        Debug.Log("<color=orange>[EnemyManager]</color> Ending enemy turn, switching to player.");
        TurnManager.PlayerTurn();
    }
}
