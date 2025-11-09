/*
 * EntityMaster.cs
 * -----------------
 * This is the main controller script for all entities (player or enemy) in the battle system.
 * It acts as a central hub that holds references to all modular components of an entity, 
 * such as health, movement, attack, death, summon, and state management.
 *
 * Responsibilities:
 * - Initialize and manage all entity sub-components.
 * - Provide a unified interface to access key entity data like grid position, faction, and materials.
 * - Set up the entity in the scene, including snapping to the grid and registering with the appropriate manager (PlayerManager or EnemyManager).
 * - Cache renderers and materials for visual effects (e.g., damage, death, summon).
 * - Maintain a reference to the Animator for playing animations across different actions.
 *
 * This design allows entity behaviors to be modular and maintainable, separating concerns 
 * between movement, combat, death, and other states while keeping the EntityMaster as the central orchestrator.
 *
 * Usage:
 * - Attach this script to any GameObject representing a battle entity.
 * - Ensure all modular components (EntityHealth, EntityAttack, EntityMovement, EntityDeath, EntitySummon, EntityState) are attached to the same GameObject.
 * - Configure the EntityData and Animator in the Inspector.
 */

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

    [HideInInspector] public Animator _animator;
    [HideInInspector] public EntityHealth health;
    [HideInInspector] public EntityAttack attack;
    [HideInInspector] public EntityDeath death;
    [HideInInspector] public EntitySummon summon;
    [HideInInspector] public EntityMovement move;
    [HideInInspector] public EntityState status;

    void Awake()
    {
        health = GetComponent<EntityHealth>();
        summon = GetComponent<EntitySummon>();
        move = GetComponent<EntityMovement>();
        attack = GetComponent<EntityAttack>();
        death = GetComponent<EntityDeath>();
        status = GetComponent<EntityState>();

        _animator = GetComponent<Animator>();

        health.Initialize(this);
        summon.Initialize(this);
        move.Initialize(this);
        attack.Initialize(this);
        death.Initialize(this);
        status.Initialize(this);
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

        move.SnapToGridPosition(gridX, gridZ);
        health.SetMaxHP();
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