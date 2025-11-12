/*
 * BattleEntityManager.cs
 * ------------------------
 * Abstract base class for managing all entities of a specific faction in a turn-based strategy game.
 *
 * Responsibilities:
 * - Maintain a list of all active entities for the faction (TeamList).
 * - Track which entity is currently selected (SelectedEntity).
 * - Provide utility methods for clearing tile highlights, resetting moves, and managing entity lifecycle.
 * - Enforce a contract for derived classes to define their faction type and implement EndTurn behavior.
 *
 * Core Features:
 * - RefreshTeam(): Rebuilds the active entity list for the faction.
 * - AddEntity()/RemoveEntity(): Manage entities dynamically during gameplay.
 * - ClearAllMoveAreas() / ClearAllAttackAreas(): Remove tile highlights.
 * - ResetEntityMoves(): Reset movement flags for all team members at the start of the turn.
 * - allTiles cache for quick access to all tiles in the scene.
 *
 * Usage:
 * - Derive specific managers (e.g., PlayerManager, EnemyManager) from this class.
 * - Implement GetFactionType() to specify the faction.
 * - Implement EndTurn() to define turn-ending behavior for the faction.
 *
 * Notes:
 * - Designed to work with GridManager and Tile classes for tile-based movement and attack range.
 * - All entity operations automatically skip dead units via the EntityMaster.status.IsDead flag.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BattleEntityManager : MonoBehaviour
{
    public List<EntityMaster> TeamList { get; protected set; } = new List<EntityMaster>();
    private EntityMaster _summoner;
    public EntityMaster SelectedEntity { get; protected set; }

    protected Tile[] allTiles;

    protected virtual void Awake()
    {
        allTiles = FindObjectsOfType<Tile>();
    }

    void Start()
    {
        RefreshTeam();

    }

    public virtual void RefreshTeam()
    {
        TeamList = FindObjectsOfType<EntityMaster>()
            .Where(e => e.data.faction == GetFactionType() && !e.status.IsDead)
            .ToList();

        Debug.Log($"[{GetType().Name}] Team refreshed. Count: {TeamList.Count}");
    }

    protected abstract Faction GetFactionType();

    public virtual void AddEntity(EntityMaster entity)
    {
        if (entity != null && entity.data.faction == GetFactionType() && !TeamList.Contains(entity))
        {
            TeamList.Add(entity);
            if (entity.data.canSummon && _summoner == null)
            {
                _summoner = entity;
                Debug.Log($"[{GetType().Name}] Summoner Added: {_summoner.data.unitName}");
            }

            Debug.Log($"[{GetType().Name}] Added: {entity.data.name}");
        }
    }

    public virtual void RemoveEntity(EntityMaster entity)
    {
        if (entity != null && TeamList.Contains(entity))
        {
            if (entity.data.canSummon)
            {
                if (entity.data.faction == Faction.PLAYER)
                {
                    TurnManager.PlayerLose();
                }
                else if (entity.data.faction == Faction.ENEMY)
                {
                    TurnManager.PlayerWin();
                }
            }

            Tile tile = FindObjectOfType<GridManager>()?.GetTileAt(entity.pos.GridX, entity.pos.GridZ);
            if (tile != null && tile.GetOccupyingEntity() == entity)
                tile.SetOccupyingEntity(null);
            TeamList.Remove(entity);

            Debug.Log($"[{GetType().Name}] Removed: {entity.name}");
        }
    }

    public virtual void ClearAllMoveAreas()
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        foreach (Tile t in grid.GetAllTiles())
        {
            t.tileMove.ClearMoveArea();
            t.tileAttack.ClearAttackArea();
        }
    }

    public virtual void ClearAllAttackAreas()
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        foreach (Tile t in grid.GetAllTiles())
            t.tileAttack.ClearAttackArea();

        Debug.Log($"[{GetType().Name}] Cleared all attack area highlights.");
    }

    public virtual void RefreshTileList()
    {
        allTiles = FindObjectsOfType<Tile>();
    }

    public virtual void ResetEntityMoves()
    {
        foreach (var entity in TeamList)
        {
            if (entity == null) continue;
            if (entity.data.canSummon) entity.soul.IncreaseSoul(entity.data.soulRecovery);

            entity.move.SetHadMove(false);
            Debug.Log($"[{GetType().Name}] Reset move for {entity.name}");
        }
    }

    public virtual EntityMaster GetSummoner()
    {
        return _summoner;
    }

    public abstract void EndTurn();
}
