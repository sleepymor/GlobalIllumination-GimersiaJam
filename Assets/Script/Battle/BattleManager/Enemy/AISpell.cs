using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class AISpell
{
    EnemyManager _m;
    public AISpell(EnemyManager m)
    {
        _m = m;
    }

    public EntityMaster FindNearestTarget(EntityMaster self, List<EntityMaster> targets)
    {
        EntityMaster nearest = null;
        float bestDist = float.MaxValue;

        foreach (var t in targets)
        {
            if (t == null || t.status.IsDead) continue;

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


    public void TryAutoSpell()
    {
        if (EnemyDeckManager.Instance == null)
        {
            Debug.LogWarning("[EnemyManager] EnemyDeckManager not found!");
            return;
        }

        var hand = EnemyDeckManager.Instance.Hand;
        if (hand == null || hand.Count == 0)
        {
            Debug.Log("[EnemyManager] No spell cards in hand.");
            return;
        }

        // Ambil semua spell di tangan
        var spellCards = hand.Where(c => c is SpellData).Cast<SpellData>().ToList();
        if (spellCards.Count == 0) return;

        // Ambil summoner / caster (biasanya unit utama musuh)
        EntityMaster caster = _m.TeamList.FirstOrDefault(e => e != null && e.data.canSummon);
        if (caster == null)
        {
            Debug.LogWarning("[EnemyManager] No caster found for spell!");
            return;
        }

        // Pilih spell random dari tangan (AI bisa ditingkatkan nanti)
        SpellData chosenSpell = spellCards[Random.Range(0, spellCards.Count)];

        if (caster.soul.GetSoulCount() < chosenSpell.summonCost)
        {
            Debug.Log($"[EnemyManager] Not enough soul for {chosenSpell.name}");
            return;
        }

        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[EnemyManager] GridManager not found!");
            return;
        }

        // === Cari target tergantung tipe spell ===
        EntityMaster targetEntity = null;
        Tile targetTile = null;

        switch (chosenSpell.DamageType)
        {
            case DamageType.DOT:
            case DamageType.Freeze:
                // Target ke unit PLAYER terdekat
                targetEntity = FindNearestTarget(caster, PlayerManager.Instance.TeamList);
                if (targetEntity != null)
                    targetTile = grid.GetTileAt(targetEntity.pos.GridX, targetEntity.pos.GridZ);
                break;

            case DamageType.AOE:
                // 🔥 Pilih tile yang mengandung musuh (PLAYER) tapi hindari tile yang mengandung ENEMY
                List<Tile> safeTiles = new List<Tile>();

                foreach (var tile in grid.GetAllTiles())
                {
                    if (!tile.isOccupied) continue;

                    // Cek apakah tile ini punya musuh (PLAYER)
                    if (tile.occupyingEntity.data.faction == Faction.PLAYER)
                    {
                        // Pastikan tidak ada musuh sendiri di area AOE ini
                        bool containsEnemy = false;
                        Queue<(Tile t, int range)> q = new Queue<(Tile, int)>();
                        HashSet<Tile> visited = new HashSet<Tile>();

                        q.Enqueue((tile, chosenSpell.aoeRange));
                        visited.Add(tile);

                        while (q.Count > 0)
                        {
                            var (current, range) = q.Dequeue();
                            if (current.isOccupied && current.occupyingEntity.data.faction == Faction.ENEMY)
                            {
                                containsEnemy = true;
                                break;
                            }

                            if (range <= 0) continue;
                            Vector2Int coords = grid.GetTileCoordinates(current);
                            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                            foreach (var dir in dirs)
                            {
                                Tile neighbor = grid.GetTileAt(coords.x + dir.x, coords.y + dir.y);
                                if (neighbor == null || visited.Contains(neighbor)) continue;
                                visited.Add(neighbor);
                                q.Enqueue((neighbor, range - 1));
                            }
                        }

                        if (!containsEnemy)
                            safeTiles.Add(tile);
                    }
                }

                // Pilih tile aman yang paling banyak berisi musuh (PLAYER)
                if (safeTiles.Count > 0)
                    targetTile = safeTiles[Random.Range(0, safeTiles.Count)];
                break;
        }

        if (targetTile == null)
        {
            Debug.Log("[EnemyManager] No valid target for spell.");
            return;
        }

        // === Cast spell ===
        Debug.Log($"[EnemyManager] {caster.name} casts {chosenSpell.name} on {targetTile.name}");

        switch (chosenSpell.DamageType)
        {
            case DamageType.DOT:
                targetEntity.status.SetPoison(chosenSpell.amount, chosenSpell.spellDuration);
                break;
            case DamageType.Freeze:
                targetEntity.status.SetStun(chosenSpell.amount, chosenSpell.spellDuration);
                break;
            case DamageType.AOE:
                targetTile.tileAttack.DealAOEDamage(chosenSpell.aoeRange, chosenSpell.amount);
                break;
        }

        caster.soul.ReduceSoul(chosenSpell.summonCost);
        EnemyDeckManager.Instance.Hand.Remove(chosenSpell);
    }


}