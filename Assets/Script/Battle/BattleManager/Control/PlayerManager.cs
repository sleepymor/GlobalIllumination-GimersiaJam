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

    public void Start()
    {
        PlayerDeckManager.Instance.InitDeck();
        PlayerDeckManager.Instance.DrawMultiple(5);
    }

    protected override Faction GetFactionType() => Faction.PLAYER;

    public void TileClicked(Tile clickedTile)
    {
        if (TurnManager.GetCurrentTurn() != Faction.PLAYER) return;

        // If clicked on a tile with a player entity that can move, select it
        if (clickedTile.isOccupied && clickedTile.occupyingEntity != null)
        {
            EntityMaster clickedEntity = clickedTile.occupyingEntity;

            if (clickedEntity.data.faction == Faction.PLAYER && !clickedEntity.move.HasMoved)
            {
                SelectEntity(clickedEntity);
                ShowMovementAndAttackAreas(clickedEntity);
                return;
            }

            // If clicked on an enemy while a player entity is selected, attempt attack
            if (SelectedEntity != null && clickedEntity.data.faction == Faction.ENEMY)
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
            currentTile.tileMove.ShowMoveAreaBFS(entity.move.MoveRange);
            currentTile.tileAttack.ShowAttackAreaBFS(entity.data.attackRange);
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
        targetTile.tileAttack.ShowAttackAreaBFS(entity.data.attackRange);

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
                tile.tileMove.ClearMoveArea();
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

    public void ClearAllMoveAndAttackAreas()
    {
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            tile.tileMove.ClearMoveArea();
            tile.tileAttack.ClearAttackArea();
            tile.tileAction.ClearActionArea();
        }
    }

    private void ClearAllAttackAreas()
    {
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            tile.tileAttack.ClearAttackArea();
        }
    }
}
