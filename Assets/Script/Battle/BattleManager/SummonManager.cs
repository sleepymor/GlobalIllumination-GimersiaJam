using UnityEngine;


/*
 * This class handle summoning, by telling this class who will summon and activating the tile near it.
 */

public class SummonManager : MonoBehaviour
{
    public static SummonManager Instance;
    private EntityMaster currentSummoner;
    private EntityData pendingSummonData;
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

    public void ShowSummonArea(EntityMaster summoner, EntityData data)
    {
        if (summoner == null || data == null)
        {
            Debug.LogWarning("[SummonManager] Summon area gagal ditampilkan: summoner/data null.");
            return;
        }

        currentSummoner = summoner;
        pendingSummonData = data;

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

    public void SummonAtTile()
    {
        if (pendingSummonData == null || currentSummoner == null)
        {
            Debug.LogWarning("[SummonManager] Tidak ada data summon yang sedang aktif.");
            return;
        }

        if (targetTile == null || targetTile.isOccupied)
        {
            Debug.LogWarning("[SummonManager] Tile target tidak valid atau sudah ditempati.");
            return;
        }

        if (!targetTile.isSummonArea)
        {
            Debug.LogWarning("[SummonManager] Tile target bukan untuk summon");
            return;
        }

        // 1️⃣ Buat unit baru
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

        // 4️⃣ Jalankan animasi summon
        newEntity.StartCoroutine(newEntity.anim.SummonAnim());

        // 5️⃣ Bersihkan area summon
        HideSummonArea();

        Debug.Log($"[SummonManager] Summoned {pendingSummonData.entityName} di ({targetTile.gridX},{targetTile.gridZ}).");
    }

}