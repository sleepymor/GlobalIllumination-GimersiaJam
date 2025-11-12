using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SpellManager : MonoBehaviour
{
    public static SpellManager Instance;
    private List<EntityMaster> unitList;
    private EntityMaster currentSummoner;
    [HideInInspector] public SpellData pendingSpellData;
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

    public void ShowActionArea(EntityMaster speller, List<EntityMaster> unitList, Card data)
    {

        if (speller == null || data == null)
        {
            Debug.LogWarning("[SpellManager] Summon area gagal ditampilkan: speller/data null.");
            return;
        }

        currentSummoner = speller;
        this.unitList = unitList;
        pendingSpellData = (SpellData)data;

        if (((SpellData)data).DamageType == DamageType.AOE) return;
        foreach (var unit in unitList)
        {
            new EntitySpell(unit).ShowActionArea((SpellData)data);
        }
    }

    public void HideActionArea()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[SpellManager] GridManager tidak ditemukan!");
            return;
        }

        foreach (Tile tile in grid.GetAllTiles())
        {
            tile.tileAction.ClearActionArea();
            tile.tileAttack.ClearAttackArea();
        }

        Debug.Log("[SpellManager] Semua area spell disembunyikan.");
    }

    public void ActivateAt(CardWrapper cardWrapper)
    {
        if (pendingSpellData == null || currentSummoner == null)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SpellManager] Tidak ada data spell yang sedang aktif.");
            pendingSpellData = null;

            return;
        }

        if (targetTile == null)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SpellManager] Tile target tidak valid atau merupakan tile player");
            pendingSpellData = null;

            return;
        }

        if (!targetTile.isActionArea)
        {
            if (pendingSpellData.DamageType != DamageType.AOE)
            {
                Debug.LogWarning("[SpellManager] Tile target bukan untuk spell");
                pendingSpellData = null;
                if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
                return;
            }
        }
        if (currentSummoner.soul.GetSoulCount() < pendingSpellData.summonCost)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SpellManager] Tidak cukup soul untuk menspell!");
            pendingSpellData = null;

            return;
        }

        DamageType spellType = pendingSpellData.DamageType;
        EntityMaster targetEntity = targetTile.GetOccupyingEntity();


        if (!targetTile.isTileHovered)
        {
            switch (spellType)
            {
                case DamageType.DOT:
                    targetEntity.status.SetPoison(pendingSpellData.amount, pendingSpellData.spellDuration);
                    break;
                case DamageType.Freeze:
                    targetEntity.status.SetStun(pendingSpellData.amount, pendingSpellData.spellDuration);
                    break;
            }
            Destroy(cardWrapper.gameObject);
            currentSummoner.soul.ReduceSoul(pendingSpellData.summonCost);
        }

        if (spellType == DamageType.AOE)
        {
            targetTile.tileAttack.DealAOEDamage(pendingSpellData.aoeRange, pendingSpellData.amount);
            Destroy(cardWrapper.gameObject);
            currentSummoner.soul.ReduceSoul(pendingSpellData.summonCost);
        }


        HideActionArea();
        pendingSpellData = null;
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