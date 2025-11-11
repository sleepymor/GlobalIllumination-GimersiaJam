using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AIEquip
{
    EnemyManager _m;
    public AIEquip(EnemyManager m)
    {
        _m = m;
    }

    public void TryAutoEquip()
    {
        if (EnemyDeckManager.Instance == null)
        {
            Debug.LogWarning("[EnemyManager] EnemyDeckManager not found!");
            return;
        }

        List<Card> enemyHand = EnemyDeckManager.Instance.Hand;
        if (enemyHand == null || enemyHand.Count == 0)
        {
            Debug.Log("[EnemyManager] Enemy has no cards to equip.");
            return;
        }

        // Cari kartu ItemData dari tangan
        var equipCards = enemyHand
            .Where(c => c is ItemData)
            .Cast<ItemData>()
            .ToList();

        if (equipCards.Count == 0)
        {
            Debug.Log("[EnemyManager] No equip cards in hand.");
            return;
        }

        // Pilih kartu secara acak (atau bisa buat AI cerdas nanti)
        ItemData chosenItem = equipCards[Random.Range(0, equipCards.Count)];

        // Cari summoner musuh (biasanya satu entitas utama)
        EntityMaster summoner = _m.TeamList.FirstOrDefault(e => e != null && e.data.canSummon);
        if (summoner == null)
        {
            Debug.LogWarning("[EnemyManager] No summoner found for enemy team!");
            return;
        }

        // Pastikan soul cukup
        if (summoner.soul.GetSoulCount() < chosenItem.summonCost)
        {
            Debug.Log($"[EnemyManager] Not enough soul to equip {chosenItem.name}");
            return;
        }

        // Pilih target entity untuk diberi buff (misal yang masih hidup)
        var aliveUnits = _m.TeamList.Where(u => u != null && !u.status.IsDead).ToList();
        if (aliveUnits.Count == 0)
        {
            Debug.Log("[EnemyManager] No valid units to equip.");
            return;
        }

        // Contoh strategi: pilih unit dengan HP paling rendah
        EntityMaster targetUnit = aliveUnits
            .OrderBy(u => u.data.currentHP)
            .FirstOrDefault();

        if (targetUnit == null)
        {
            Debug.Log("[EnemyManager] No target found for equip.");
            return;
        }

        // Jalankan efek berdasarkan buff type
        switch (chosenItem.BuffType)
        {
            case BuffType.Heal:
                targetUnit.health.Heal(chosenItem.amount);
                break;
            case BuffType.Attack:
                targetUnit.equip.AttackBuff(chosenItem.amount);
                break;
            case BuffType.Defense:
                targetUnit.equip.DefenseBuff(chosenItem.amount);
                break;
            case BuffType.Crit:
                targetUnit.equip.CritBuff(chosenItem.amount);
                break;
        }

        // Kurangi soul dan hapus kartu dari tangan
        summoner.soul.ReduceSoul(chosenItem.summonCost);
        EnemyDeckManager.Instance.Hand.Remove(chosenItem);

        Debug.Log($"[EnemyManager] Enemy equipped {chosenItem.name} on {targetUnit.name}");
    }

}