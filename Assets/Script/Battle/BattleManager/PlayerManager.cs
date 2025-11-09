/*
 * PlayerManager.cs
 * ------------------------
 * Singleton class that manages all player-controlled entities in a turn-based strategy game.
 * Inherits from BattleEntityManager to leverage generic team and tile management.
 *
 * Responsibilities:
 * - Maintain a list of all active player units in the scene.
 * - Handle player input: selecting units, moving them, and attacking enemies.
 * - Manage movement and attack range highlights on the grid.
 * - Ensure units cannot move or attack more than once per turn.
 * - End the player’s turn and pass control to the enemy.
 *
 * Core Features:
 * - TileClicked(Tile): Main handler for when a tile is clicked, managing selection, movement, and attacks.
 * - SelectEntity(EntityMaster): Selects a player unit and clears previous highlights.
 * - ShowMovementAndAttackAreas(EntityMaster): Highlights possible movement and attack tiles.
 * - TryAttack(EntityMaster, EntityMaster): Executes an attack if the target is in range.
 * - MoveAndEnableAttack(EntityMaster, Tile): Coroutine for moving a unit and showing attack options afterward.
 * - ClearMoveAreasOnly(): Clears only movement tiles, keeping attack tiles highlighted.
 * - ClearAllMoveAndAttackAreas(): Clears all movement and attack highlights.
 *
 * Notes:
 * - Designed to work closely with Tile, GridManager, and EntityMaster classes.
 * - Keeps track of a SelectedEntity for turn-based actions.
 * - Movement and attack BFS logic are delegated to Tile.
 *
 * Usage:
 * - Attach this script to a singleton PlayerManager GameObject in the scene.
 * - Tiles automatically call TileClicked() when clicked.
 * - EndTurn() should be called to switch control to the enemy.
 */

using UnityEngine;
using System.Collections;

public class PlayerManager : BattleEntityManager
{
    public static PlayerManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override Faction GetFactionType() => Faction.PLAYER;

    public void TileClicked(Tile clickedTile)
    {
        if (TurnManager.GetCurrentTurn() != Faction.PLAYER) return;

        // If clicked on a tile with a player entity that can move, select it
        if (clickedTile.isOccupied && clickedTile.occupyingEntity != null)
        {
            EntityMaster clickedEntity = clickedTile.occupyingEntity;

            if (clickedEntity.Faction == Faction.PLAYER && !clickedEntity.move.HasMoved)
            {
                SelectEntity(clickedEntity);
                ShowMovementAndAttackAreas(clickedEntity);
                return;
            }

            // If clicked on an enemy while a player entity is selected, attempt attack
            if (SelectedEntity != null && clickedEntity.Faction == Faction.ENEMY)
            {
                TryAttack(SelectedEntity, clickedEntity);
                return;
            }
        }

        // If clicked on a movement tile
        if (clickedTile.isMoveArea && SelectedEntity != null)
        {
            StartCoroutine(MoveAndEnableAttack(SelectedEntity, clickedTile));
            return;
        }

        // Otherwise, clear selection and highlights
        ClearAllMoveAndAttackAreas();
    }

    private void SelectEntity(EntityMaster entity)
    {
        ClearAllMoveAndAttackAreas();
        SelectedEntity = entity;
    }

    private void ShowMovementAndAttackAreas(EntityMaster entity)
    {
        Tile currentTile = entity.currentTile;
        if (currentTile != null)
        {
            currentTile.ShowMoveAreaBFS(entity.move.MoveRange);
            currentTile.ShowAttackAreaBFS(entity.data.attackRange);
        }
    }

    private void TryAttack(EntityMaster attacker, EntityMaster target)
    {
        if (attacker.attack.CanAttack(target))
        {
            attacker.attack.Attack(target);
            attacker.attack.SetHadAttacking(true);

            // After attack, deselect
            SelectedEntity = null;
            ClearAllAttackAreas();
        }
        else
        {
            Debug.Log("[PlayerManager] Target out of range!");
        }
    }

    private IEnumerator MoveAndEnableAttack(EntityMaster entity, Tile targetTile)
    {
        // Move to the target tile
        yield return entity.StartCoroutine(
            entity.move.MoveToGridPosition(targetTile.gridX, targetTile.gridZ)
        );

        // Mark as moved
        entity.move.SetHadMove(true);

        // Show attack area after moving
        targetTile.ShowAttackAreaBFS(entity.data.attackRange);

        // Keep the entity selected so player can attack
        SelectedEntity = entity;

        // Clear only movement highlights
        ClearMoveAreasOnly();
    }

    // Clears only move tiles, leaves attack tiles
    private void ClearMoveAreasOnly()
    {
        // Implementation depends on your Tile class
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            if (tile.isMoveArea)
                tile.ClearMoveArea();
        }
    }

    public void SetSelectedEntity(EntityMaster entity)
    {
        SelectedEntity = entity;
    }

    public override void EndTurn()
    {
        if (TurnManager.GetCurrentTurn() != Faction.PLAYER) return;
        TurnManager.EnemyTurn();
    }

    private void ClearAllMoveAndAttackAreas()
    {
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            tile.ClearMoveArea();
            tile.ClearAttackArea();
        }
    }

    private void ClearAllAttackAreas()
    {
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            tile.ClearAttackArea();
        }
    }
}
