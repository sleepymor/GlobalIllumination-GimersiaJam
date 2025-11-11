using UnityEngine;
using System.Collections;

/*
 * This class handle summoning, by telling this class who will summon and activating the tile near it.
 */

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance;
    private EntityMaster currentSummoner;
    private UnitData pendingSummonData;
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

    public void ShowSummonArea(EntityMaster summoner, Card data)
    {
        if (summoner == null || data == null)
        {
            Debug.LogWarning("[SummonManager] Summon area gagal ditampilkan: summoner/data null.");
            return;
        }

        currentSummoner = summoner;
        pendingSummonData = (UnitData)data;

        new EntitySummon(summoner).ShowSummonArea();
    }

    public void HideSummonArea()
    {
        if (TurnManager.GetCurrentTurn() == Faction.PLAYER)
        {
            EntityMaster summoner = PlayerManager.Instance.GetSummoner();
            summoner.summon.HideSummonArea();
        }
    }

    public void SummonAtTile(CardWrapper cardWrapper)
    {
        if (pendingSummonData == null || currentSummoner == null)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tidak ada data summon yang sedang aktif.");
            return;
        }

        if (targetTile == null || targetTile.isOccupied)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tile target tidak valid atau sudah ditempati.");
            return;
        }

        if (!targetTile.isActionArea)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tile target bukan untuk summon");
            return;
        }

        if (currentSummoner.soul.GetSoulCount() < pendingSummonData.summonCost)
        {
            if (cardWrapper != null) StartCoroutine(BlinkCardRed(cardWrapper));
            Debug.LogWarning("[SummonManager] Tidak cukup soul untuk mensummon!");
            return;
        }

        if (targetTile.isTileHovered)
        {
            GameObject newUnit = GameObject.Instantiate(
          pendingSummonData.prefab,
          targetTile.transform.position,
          Quaternion.identity
      );

            EntityMaster newEntity = newUnit.GetComponent<EntityMaster>();

            // 2️⃣ Hubungkan tile & posisi grid
            int x = targetTile.gridX;
            int z = targetTile.gridZ;
            newEntity.pos.SetPos(x, z);
            newEntity.currentTile = targetTile;
            targetTile.SetOccupyingEntity(newEntity);

            // 3️⃣ Tambahkan ke tim player
            PlayerManager.Instance.AddEntity(newEntity);
            newEntity.data.faction = currentSummoner.data.faction;
            newEntity.data.canSummon = false;
            // 4️⃣ Jalankan animasi summon
            newEntity.StartCoroutine(newEntity.anim.SummonAnim());

            currentSummoner.soul.ReduceSoul(pendingSummonData.summonCost);
            Destroy(cardWrapper.gameObject);
        }

        // 1️⃣ Buat unit baru


        // 5️⃣ Bersihkan area summon
        HideSummonArea();
        pendingSummonData = null;

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