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
        Debug.Log($"<color=orange>[EnemyManager]</color> Found {TeamList.Count} enemy entities in the scene.");

        if (TeamList.Count == 0)
        {
            Debug.LogWarning("[EnemyManager] No enemies to act. Ending turn early.");
            TurnManager.PlayerTurn();
            yield break;
        }

        foreach (var enemy in TeamList)
        {
            if (enemy == null)
            {
                Debug.LogWarning("[EnemyManager] Null enemy entry found — skipping.");
                continue;
            }

            Debug.Log($"<color=yellow>[EnemyManager]</color> Processing enemy: {enemy.name}");

            if (enemy.IsDead)
            {
                Debug.Log($"[EnemyManager] {enemy.name} is dead. Skipping.");
                continue;
            }

            if (enemy.HasMoved)
            {
                Debug.Log($"[EnemyManager] {enemy.name} has already moved. Skipping.");
                continue;
            }

            // Find nearest player
            var playerList = PlayerManager.Instance.TeamList;
            if (playerList == null || playerList.Count == 0)
            {
                Debug.LogWarning("[EnemyManager] No player entities found! Ending enemy turn.");
                yield break;
            }

            EntityMaster target = FindNearestTarget(enemy, playerList);
            if (target == null)
            {
                Debug.LogWarning($"[EnemyManager] {enemy.name} found no valid player targets!");
                continue;
            }

            Debug.Log($"<color=cyan>[EnemyManager]</color> {enemy.name} targets {target.name}");

            // Move closer
            Tile moveTile = FindClosestTileTowardsTarget(enemy, target);
            if (moveTile != null)
            {
                Debug.Log($"<color=green>[EnemyManager]</color> {enemy.name} moving to tile ({moveTile.gridX}, {moveTile.gridZ})");
                yield return enemy.StartCoroutine(enemy.MoveToGridPosition(moveTile.gridX, moveTile.gridZ));
            }
            else
            {
                Debug.Log($"[EnemyManager] {enemy.name} found no reachable tile to move to.");
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("<color=orange>[EnemyManager]</color> Enemy turn ended.");
        TurnManager.PlayerTurn();
    }

    private EntityMaster FindNearestTarget(EntityMaster self, List<EntityMaster> targets)
    {
        EntityMaster nearest = null;
        float bestDist = float.MaxValue;

        foreach (var t in targets)
        {
            if (t == null || t.IsDead) continue;

            float dist = Vector3.Distance(self.transform.position, t.transform.position);
            Debug.Log($"[EnemyManager] Distance from {self.name} → {t.name}: {dist:F2}");

            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = t;
            }
        }

        if (nearest != null)
            Debug.Log($"[EnemyManager] {self.name} selected nearest target: {nearest.name} ({bestDist:F2} units away)");
        else
            Debug.LogWarning($"[EnemyManager] {self.name} found no alive player targets!");

        return nearest;
    }

    private Tile FindClosestTileTowardsTarget(EntityMaster self, EntityMaster target)
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null)
        {
            Debug.LogError("[EnemyManager] GridManager not found in scene!");
            return null;
        }

        Tile startTile = grid.GetTileAt(self.GridX, self.GridZ);
        Tile targetTile = grid.GetTileAt(target.GridX, target.GridZ);

        if (startTile == null || targetTile == null)
        {
            Debug.LogWarning($"[EnemyManager] Missing start or target tile for {self.name} or {target.name}");
            return null;
        }

        Debug.Log($"[EnemyManager] {self.name} searching for reachable tiles within range {self.MoveRange} from ({startTile.gridX}, {startTile.gridZ})");

        Queue<(Tile tile, int remainingRange)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();
        List<Tile> reachable = new List<Tile>();

        queue.Enqueue((startTile, self.MoveRange));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (current, rangeLeft) = queue.Dequeue();

            // Don't expand further if we ran out of movement range
            if (rangeLeft <= 0)
                continue;

            foreach (Tile neighbor in grid.GetNeighbors(current))
            {
                if (neighbor == null)
                {
                    Debug.LogWarning("[EnemyManager] Neighbor is null!");
                    continue;
                }

                if (visited.Contains(neighbor))
                    continue;

                if (neighbor.isOccupied)
                {
                    Debug.Log($"[EnemyManager] Skipping occupied tile ({neighbor.gridX}, {neighbor.gridZ})");
                    continue;
                }

                int newRange = rangeLeft - neighbor.moveCost;
                if (newRange < 0)
                {
                    Debug.Log($"[EnemyManager] Skipping ({neighbor.gridX},{neighbor.gridZ}) - insufficient range (need {neighbor.moveCost}, have {rangeLeft})");
                    continue;
                }

                visited.Add(neighbor);
                queue.Enqueue((neighbor, newRange));
                reachable.Add(neighbor);

                Debug.Log($"[EnemyManager] Added ({neighbor.gridX},{neighbor.gridZ}) → remaining range: {newRange}");
            }
        }

        if (reachable.Count == 0)
        {
            Debug.Log($"[EnemyManager] {self.name} found no reachable tiles.");
            return null;
        }

        // Choose tile closest to target
        Tile bestTile = reachable
            .OrderBy(t => Vector3.Distance(t.transform.position, targetTile.transform.position))
            .FirstOrDefault();

        if (bestTile != null)
            Debug.Log($"[EnemyManager] {self.name} best tile toward {target.name}: ({bestTile.gridX}, {bestTile.gridZ})");

        return bestTile;
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
