using UnityEngine;

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

        // 1️⃣ Select player entity
        if (clickedTile.isOccupied && clickedTile.occupyingEntity != null)
        {
            EntityMaster clickedEntity = clickedTile.occupyingEntity;

            // Selecting a friendly unit
            if (clickedEntity.Faction == Faction.PLAYER && !clickedEntity.HasMoved)
            {
                ClearAllMoveAreas();
                SelectedEntity = clickedEntity;

                // Show move & attack range
                clickedTile.ShowMoveAreaBFS(clickedEntity.MoveRange);
                clickedTile.ShowAttackAreaBFS(clickedEntity.data.attackRange);
                return;
            }

            // Attacking an enemy unit
            if (SelectedEntity != null && clickedEntity.Faction == Faction.ENEMY)
            {
                if (SelectedEntity.CanAttack(clickedEntity))
                {
                    SelectedEntity.Attack(clickedEntity);
                    SelectedEntity.SetHadAttacking(true);
                    ClearAllMoveAreas();
                    SelectedEntity = null;
                    return;
                }
                else
                {
                    Debug.Log("[PlayerManager] Target out of range!");
                    return;
                }
            }
        }

        if (clickedTile.isMoveArea && SelectedEntity != null)
        {
            SelectedEntity.StartCoroutine(SelectedEntity.MoveToGridPosition(clickedTile.gridX, clickedTile.gridZ));
            ClearAllMoveAreas();
            SelectedEntity = null;
            return;
        }

        ClearAllMoveAreas();
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
}
