using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    private List<EntityMaster> unitList;
    private EntityMaster currentSummoner;
    private ItemData pendingItemData;
    [HideInInspector] public Tile targetTile;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowEquipArea(EntityMaster summoner, List<EntityMaster> unitList, Card data)
    {
        foreach (var unit in unitList)
        {
            new EntityItem(unit).ShowEquipArea();
        }

        if (summoner == null || data == null)
        {
            Debug.LogWarning("[SummonManager] Summon area gagal ditampilkan: summoner/data null.");
            return;
        }

        currentSummoner = summoner;
        this.unitList = unitList;
        pendingItemData = (ItemData)data;
    }

    public void HideEquipArea()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[EntitySummon] GridManager tidak ditemukan!");
            return;
        }

        foreach (Tile tile in grid.GetAllTiles())
        {
            tile.ClearActionArea();
        }

        Debug.Log("[EntitySummon] Semua area summon disembunyikan.");
    }

    public void EquipAt(CardWrapper cardWrapper)
    {
        Debug.Log("test");
        if (pendingItemData == null || currentSummoner == null)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tidak ada data summon yang sedang aktif.");
            return;
        }

        if (targetTile == null || !targetTile.isOccupied)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tile target tidak valid atau tidak ada target.");
            return;
        }

        if (!targetTile.isActionArea)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tile target bukan untuk summon");
            return;
        }

        if (currentSummoner.soul.GetSoulCount() < pendingItemData.summonCost)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tidak cukup soul untuk mensummon!");
            return;
        }

        EntityMaster targetEntity = targetTile.GetOccupyingEntity();

        BuffType buffType = pendingItemData.BuffType;

        switch (buffType)
        {
            case BuffType.Heal:
                targetEntity.health.Heal(pendingItemData.amount);
                break;
            case BuffType.Attack:
                targetEntity.equip.AttackBuff(pendingItemData.amount);
                break;
            case BuffType.Defense:
                targetEntity.equip.DefenseBuff(pendingItemData.amount);
                break;
            case BuffType.Crit:
                targetEntity.equip.CritBuff(pendingItemData.amount);
                break;
        }

        HideEquipArea();
        currentSummoner.soul.ReduceSoul(pendingItemData.summonCost);
        Destroy(cardWrapper.gameObject);
        pendingItemData = null;
    }


    private IEnumerator BlinkCardRed(CardWrapper card, float duration = 0.3f)
    {
        if (card == null) yield break;

        var renderer = card.GetComponent<SpriteRenderer>();
        if (renderer == null) yield break;

        Color originalColor = renderer.color;
        Color blinkColor = Color.red;

        float elapsed = 0f;
        float halfDuration = duration / 2f;

        while (elapsed < halfDuration)
        {
            renderer.color = Color.Lerp(originalColor, blinkColor, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        renderer.color = blinkColor;

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            renderer.color = Color.Lerp(blinkColor, originalColor, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        renderer.color = originalColor;
    }


}