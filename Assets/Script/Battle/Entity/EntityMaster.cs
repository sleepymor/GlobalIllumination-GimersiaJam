using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMaster : MonoBehaviour
{

    [Header("Entity Data")]
    public EntityData data;

    [Header("Entity Grid Position")]
    [SerializeField] public int gridX;
    [SerializeField] public int gridZ;
    [SerializeField] public float heightAboveTile = 1f;
    [SerializeField] public float moveSpeed = 3f;

    public int GridX => gridX;
    public int GridZ => gridZ;

    public Faction Faction => data.faction;

    private Renderer[] renderers;
    [HideInInspector] public GridManager gridManager;
    [HideInInspector] public Tile currentTile;
    [HideInInspector] public Material[] materials;


    [HideInInspector] public EntityHealth healthManager;
    [HideInInspector] public EntityAttack attackManager;
    [HideInInspector] public EntityDeath deathManager;
    [HideInInspector] public EntitySummon summonManager;
    [HideInInspector] public EntityMovement movementManager;
    [HideInInspector] public EntityState statusManager;

    void Awake()
    {
        healthManager = GetComponent<EntityHealth>();
        summonManager = GetComponent<EntitySummon>();
        movementManager = GetComponent<EntityMovement>();
        attackManager = GetComponent<EntityAttack>();
        deathManager = GetComponent<EntityDeath>();
        statusManager = GetComponent<EntityState>();

        healthManager.Initialize(this);
        summonManager.Initialize(this);
        movementManager.Initialize(this);
        attackManager.Initialize(this);
        deathManager.Initialize(this);
        statusManager.Initialize(this);
    }

    private void Start()
    {

        gridManager = FindObjectOfType<GridManager>();

        if (gridManager == null)
        {
            Debug.LogError("[EntityMaster] No GridManager found in scene!");
            return;
        }
        renderers = GetComponentsInChildren<Renderer>();
        CacheMaterials();

        movementManager.SnapToGridPosition(gridX, gridZ);
        healthManager.SetMaxHP();
        SetManager();
    }

    private void SetManager()
    {
        if (data.faction == Faction.PLAYER) PlayerManager.Instance.AddEntity(this);
        if (data.faction == Faction.ENEMY) EnemyManager.Instance.AddEntity(this);
    }

    private void CacheMaterials()
    {
        if (renderers == null) return;

        materials = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            materials[i] = renderers[i].material;
    }

}