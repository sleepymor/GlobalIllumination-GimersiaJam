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
    public EntityMaster SelectedEntity { get; protected set; }

    protected Tile[] allTiles;

    protected virtual void Awake()
    {
        allTiles = FindObjectsOfType<Tile>();
        RefreshTeam();
    }

    /// <summary>
    /// Rebuilds the list of all entities for this faction currently in the scene.
    /// </summary>
    public virtual void RefreshTeam()
    {
        TeamList = FindObjectsOfType<EntityMaster>()
            .Where(e => e.data.faction == GetFactionType() && !e.status.IsDead)
            .ToList();

        Debug.Log($"[{GetType().Name}] Team refreshed. Count: {TeamList.Count}");
    }

    /// <summary>
    /// Returns the faction managed by this class.
    /// </summary>
    protected abstract Faction GetFactionType();

    public virtual void AddEntity(EntityMaster entity)
    {
        if (entity != null && entity.data.faction == GetFactionType() && !TeamList.Contains(entity))
        {
            TeamList.Add(entity);
            Debug.Log($"[{GetType().Name}] Added: {entity.name}");
        }
    }

    public virtual void RemoveEntity(EntityMaster entity)
    {
        if (entity != null && TeamList.Contains(entity))
        {
            TeamList.Remove(entity);

            // Clear the tile if still linked
            Tile tile = FindObjectOfType<GridManager>()?.GetTileAt(entity.pos.GridX, entity.pos.GridZ);
            if (tile != null && tile.GetOccupyingEntity() == entity)
                tile.SetOccupyingEntity(null);

            Debug.Log($"[{GetType().Name}] Removed: {entity.name}");
        }
    }


    /// <summary>
    /// Clears both move and attack highlights from all tiles.
    /// </summary>
    public virtual void ClearAllMoveAreas()
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        foreach (Tile t in grid.GetAllTiles())
        {
            t.ClearMoveArea();
            t.ClearAttackArea();
        }
    }

    /// <summary>
    /// Clears only attack highlights (red tiles).
    /// </summary>
    public virtual void ClearAllAttackAreas()
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        foreach (Tile t in grid.GetAllTiles())
            t.ClearAttackArea();

        Debug.Log($"[{GetType().Name}] Cleared all attack area highlights.");
    }

    public virtual void RefreshTileList()
    {
        allTiles = FindObjectsOfType<Tile>();
    }

    /// <summary>
    /// Resets movement flags for all units in this team at the start of a turn.
    /// </summary>
    public virtual void ResetEntityMoves()
    {
        foreach (var entity in TeamList)
        {
            if (entity == null) continue;

            entity.move.SetHadMove(false);
            Debug.Log($"[{GetType().Name}] Reset move for {entity.name}");
        }
    }

    public abstract void EndTurn();
}
