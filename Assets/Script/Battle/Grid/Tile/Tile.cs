/*
 * Tile.cs
 * -----------------
 * This class represents a single tile on the battle grid in a turn-based strategy system.
 * It handles tile-specific data, visuals, and interactions with the player and game managers.
 *
 * Responsibilities:
 * - Store tile data, including grid coordinates, move cost, and whether it is occupied.
 * - Manage visual elements such as base color, hover effects, and move/attack highlights.
 * - Handle input events (clicks) via IPointerClickHandler, allowing the player to select units, move, or attack.
 * - Compute movement and attack ranges using BFS algorithms to highlight reachable tiles.
 * - Track occupancy by an EntityMaster, and provide utility functions for clearing or activating move/attack areas.
 *
 * Notes:
 * - The tile communicates directly with PlayerManager to handle selection and actions.
 * - Movement and attack BFS logic are included in this class; for larger projects, this could be refactored into a separate helper.
 * - Hover effects and visual highlights are modular via GameObjects, making it easy to customize appearance.
 *
 * Usage:
 * - Attach this script to a tile GameObject in the scene.
 * - Assign TileData and the optional hover/move/attack objects in the Inspector.
 * - Initialize the tile via Init() to set its base color and offset.
 * - Tiles automatically respond to player clicks during their faction’s turn.
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer), typeof(Collider))]
public class Tile : MonoBehaviour, IPointerClickHandler
{
    [Header("Tile Data")]
    public TileData tileData;

    [Header("Tile Colors")]
    [SerializeField] private Color _baseColor = Color.white;
    [SerializeField] private Color _offsetColor = Color.gray;

    [Header("Hover/Move/Attack/Summon Objects")]
    [SerializeField] public GameObject _hoverObject;
    [SerializeField] public GameObject _moveAreaObject;
    [SerializeField] public GameObject _attackAreaObject;
    [SerializeField] public GameObject _actionAreaObject;

    [HideInInspector] public bool isTileHovered, isMoveArea, isAttackArea, isActionArea;

    private Renderer _renderer;
    private Material _materialInstance;
    private Color _originalColor;

    [HideInInspector] public int gridX, gridZ;
    [HideInInspector] public bool isOccupied = false;
    [HideInInspector] public EntityMaster occupyingEntity;

    [HideInInspector] public TileMoveUtils tileMove;
    [HideInInspector] public TileAttackUtils tileAttack;
    [HideInInspector] public TileActionUtils tileAction;
    [HideInInspector] public GridManager grid;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materialInstance = new Material(_renderer.sharedMaterial);
        _renderer.material = _materialInstance;

        isMoveArea = tileData.isMoveArea;

        _hoverObject?.SetActive(false);
        _moveAreaObject?.SetActive(false);
        _attackAreaObject?.SetActive(false);

        tileMove = new TileMoveUtils(this);
        tileAttack = new TileAttackUtils(this);
        tileAction = new TileActionUtils(this);

        grid = FindFirstObjectByType<GridManager>();
    }

    public void Init(bool isOffset)
    {
        _originalColor = isOffset ? _offsetColor : _baseColor;
        _materialInstance.color = _originalColor;
    }

    private void OnMouseEnter()
    {
        if (_hoverObject != null)
        {
            _hoverObject.SetActive(true);
            SummonManager.Instance.targetTile = this;
            ItemManager.Instance.targetTile = this;
            SpellManager.Instance.targetTile = this;
        }

        if (SpellManager.Instance.pendingSpellData != null)
        {
            SpellData data = SpellManager.Instance.pendingSpellData;
            if (data.DamageType != DamageType.AOE) return;
            int aoeRange = data.aoeRange;

            tileAttack.ShowAOEAreaBFS(aoeRange);
        }
        isTileHovered = true;
    }

    private void OnMouseExit()
    {
        if (_hoverObject != null)
        {
            _hoverObject.SetActive(false);
        }

        if (SpellManager.Instance.pendingSpellData != null)
        {
            SpellData data = SpellManager.Instance.pendingSpellData;

            if (data.DamageType == DamageType.AOE)
            {
                tileAttack.ClearAOEArea();
            }
        }
        isTileHovered = false;
        SummonManager.Instance.targetTile = this;
        ItemManager.Instance.targetTile = this;
        SpellManager.Instance.targetTile = this;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Faction currentTurn = TurnManager.GetCurrentTurn();
        if (currentTurn != Faction.PLAYER) return;

        var playerManager = PlayerManager.Instance;

        // 1️⃣ Klik tile yang ada unit milik player → pilih unit & tampilkan area gerak + serang
        if (isOccupied && occupyingEntity != null && occupyingEntity.data.faction == Faction.PLAYER)
        {
            // Cegah pemilihan unit yang sudah menyerang, tapi masih izinkan lihat area gerak
            playerManager.ClearAllMoveAndAttackAreas();
            playerManager.SetSelectedEntity(occupyingEntity);

            // Selalu tampilkan area gerak kalau unit belum move
            if (!occupyingEntity.move.HasMoved)
                tileMove.ShowMoveAreaBFS(occupyingEntity.move.MoveRange);

            // Selalu tampilkan area serang kalau belum menyerang
            if (!occupyingEntity.attack.IsAlreadyAttacking)
                tileAttack.ShowAttackAreaBFS(occupyingEntity.attack.AttackRange);

            return;
        }

        // 2️⃣ Klik tile gerak → pindahkan unit ke tile itu
        if (isMoveArea && playerManager.SelectedEntity != null)
        {
            var entity = playerManager.SelectedEntity;
            playerManager.TileClicked(this);
            playerManager.ClearAllMoveAndAttackAreas();
            return;
        }

        // 3️⃣ Klik tile serang → lakukan serangan
        if (isAttackArea && playerManager.SelectedEntity != null)
        {
            var attacker = playerManager.SelectedEntity;

            if (isOccupied && occupyingEntity != null && occupyingEntity.data.faction != attacker.data.faction)
            {
                Debug.Log($"[Tile] {attacker.name} attacks {occupyingEntity.name}!");
                attacker.attack.Attack(occupyingEntity);
            }

            playerManager.SetSelectedEntity(null);
            return;
        }

        // 4️⃣ Klik tile kosong di luar area → bersihkan semua area
        playerManager.ClearAllMoveAndAttackAreas();
    }


    public void SetOccupyingEntity(EntityMaster entity)
    {
        occupyingEntity = entity;
        isOccupied = entity != null;
    }

    public void SetOccupied(bool value)
    {
        isOccupied = value;
    }

    public Vector3 GetCenterPosition(float heightOffset = 0f)
    {
        Vector3 pos = transform.position;
        return new Vector3(pos.x, pos.y + heightOffset, pos.z);
    }


    public EntityMaster GetOccupyingEntity() => occupyingEntity;

}
