using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EntityMaster : MonoBehaviour
{
    [Header("Entity Data")]
    public EntityData data;

    [SerializeField] private int spawnPosX;
    [SerializeField] private int spawnPosZ;
    [SerializeField] private float heightAboveTile = 0.5f;

    private Renderer[] renderers;
    [HideInInspector] public GridManager gridManager;
    [HideInInspector] public Tile currentTile;
    [HideInInspector] public Material[] materials;
    [HideInInspector] public EntityHealth health;
    [HideInInspector] public EntityAttack attack;
    [HideInInspector] public EntitySummon summon;
    [HideInInspector] public EntityMovement move;
    [HideInInspector] public EntityState status;
    [HideInInspector] public EntityEquip equip;
    [HideInInspector] public EntityAnim anim;
    [HideInInspector] public EntityManager manager;
    [HideInInspector] public EntityPosition pos;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        // Coba cari GridManager di scene
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();

        // Kalau belum ada grid, keluar
        if (gridManager == null || gridManager.GetTileAt(spawnPosX, spawnPosZ) == null)
            return;

        // Pindahkan entity ke posisi tile yang sesuai
        Tile tile = gridManager.GetTileAt(spawnPosX, spawnPosZ);
        Vector3 targetPos = tile.transform.position + Vector3.up * heightAboveTile;

        // Hanya update posisi kalau benar-benar berubah (biar tidak spam redraw)
        if (transform.position != targetPos)
        {
            transform.position = targetPos;
            currentTile = tile;

#if UNITY_EDITOR
            // biar posisi tersimpan ke scene
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
#endif

    void Awake()
    {
        if (!Application.isPlaying) return;

        data = Instantiate(data);

        attack = new EntityAttack(this);
        move = new EntityMovement(this);
        summon = new EntitySummon(this);
        health = new EntityHealth(this);
        status = new EntityState(this);
        equip = new EntityEquip(this);
        pos = new EntityPosition(spawnPosX, spawnPosZ);

        anim = new EntityAnim(this, GetComponent<Animator>());
    }

    private void Start()
    {
        if (!Application.isPlaying) return;

        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
        {
            Debug.LogError("[EntityMaster] No GridManager found in scene!");
            return;
        }

        renderers = GetComponentsInChildren<Renderer>();
        CacheMaterials();

        manager = new EntityManager(this);

        move.SnapToGridPosition(pos.gridX, pos.gridZ);
        health.SetMaxHP();
    }

    private void CacheMaterials()
    {
        if (renderers == null) return;

        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;
    }
}
