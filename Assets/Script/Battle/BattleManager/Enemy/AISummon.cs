using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AISummon
{
    EnemyManager _m;
    public AISummon(EnemyManager m)
    {
        _m = m;
    }


    public void TryAutoSummon()
    {
        if (EnemyDeckManager.Instance == null)
        {
            Debug.LogWarning("[EnemyManager] EnemyDeckManager not found!");
            return;
        }

        // Get enemy hand
        List<Card> enemyHand = EnemyDeckManager.Instance.Hand;
        if (enemyHand == null || enemyHand.Count == 0)
        {
            Debug.Log("[EnemyManager] Enemy has no cards to summon.");
            return;
        }

        // Find enemy summoner (assuming one per team)
        EntityMaster summoner = _m.TeamList.FirstOrDefault(e => e != null && e.data.canSummon);
        if (summoner == null)
        {
            Debug.LogWarning("[EnemyManager] No summoner found for enemy team!");
            return;
        }

        // Get all summonable cards (UnitData)
        var summonableCards = enemyHand
            .Where(c => c is UnitData unit && unit.prefab != null)
            .Cast<UnitData>()
            .ToList();

        if (summonableCards.Count == 0)
        {
            Debug.Log("[EnemyManager] No summonable cards in hand.");
            return;
        }

        // Choose one randomly or use your own AI strategy
        UnitData chosenCard = summonableCards[Random.Range(0, summonableCards.Count)];

        // Check if we have enough souls to summon it
        if (summoner.soul.GetSoulCount() < chosenCard.summonCost)
        {
            Debug.Log($"[EnemyManager] Not enough soul to summon {chosenCard.name}");
            return;
        }

        // Find valid summon tiles near the summoner
        GridManager grid = GridManager.Instance;
        Tile startTile = grid.GetTileAt(summoner.pos.GridX, summoner.pos.GridZ);
        if (startTile == null)
        {
            Debug.LogWarning("[EnemyManager] Summoner start tile not found!");
            return;
        }

        int summonRange = summoner.data.summonRange;
        List<Tile> summonTiles = GetSummonableTiles(startTile, summonRange)
            .Where(t => !t.isOccupied)
            .ToList();

        if (summonTiles.Count == 0)
        {
            Debug.Log("[EnemyManager] No available tiles for summon.");
            return;
        }

        Tile targetTile = summonTiles[Random.Range(0, summonTiles.Count)];

        // Instantiate the summoned unit
        GameObject newUnit = GameObject.Instantiate(
            chosenCard.prefab,
            targetTile.transform.position,
            Quaternion.identity
        );

        EntityMaster newEntity = newUnit.GetComponent<EntityMaster>();
        if (newEntity == null)
        {
            Debug.LogError("[EnemyManager] Summoned prefab missing EntityMaster component!");
            return;
        }

        // Assign tile and grid position
        newEntity.pos.SetPos(targetTile.gridX, targetTile.gridZ);
        newEntity.currentTile = targetTile;
        targetTile.SetOccupyingEntity(newEntity);

        // Assign faction and add to team
        newEntity.data.faction = Faction.ENEMY;
        newEntity.data.canSummon = false;
        _m.AddEntity(newEntity);

        // Reduce soul and remove card from hand
        summoner.soul.ReduceSoul(chosenCard.summonCost);
        EnemyDeckManager.Instance.Hand.Remove(chosenCard);

        // Play summon animation if any
        newEntity.StartCoroutine(newEntity.anim.SummonAnim());

        Debug.Log($"[EnemyManager] Enemy summoned {chosenCard.name} at tile ({targetTile.gridX},{targetTile.gridZ}).");
    }

    private List<Tile> GetSummonableTiles(Tile startTile, int range)
    {
        List<Tile> result = new List<Tile>();
        GridManager grid = GridManager.Instance;
        if (grid == null || startTile == null) return result;

        Queue<(Tile tile, int rangeLeft)> queue = new Queue<(Tile, int)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        queue.Enqueue((startTile, range));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (current, remaining) = queue.Dequeue();
            if (!current.isOccupied)
                result.Add(current);

            if (remaining <= 0) continue;

            foreach (Tile neighbor in grid.GetNeighbors(current))
            {
                if (neighbor == null || visited.Contains(neighbor)) continue;

                int newRange = remaining - neighbor.tileMove.moveCost;
                if (newRange < 0) continue;

                visited.Add(neighbor);
                queue.Enqueue((neighbor, newRange));
            }
        }

        return result;
    }

}