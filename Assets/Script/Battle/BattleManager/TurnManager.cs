using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Turn Settings")]
    public bool IsPlayerTurn = true;

    public static EntityMaster SelectedEntity;

    private Tile[] allTiles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        allTiles = FindObjectsOfType<Tile>();
    }

    private void Start()
    {
        SelectedEntity = null;
    }

    public void TileClicked(Tile clickedTile)
    {
        // Case 1: clicking a tile with a player entity
        if (clickedTile.isOccupied && clickedTile.occupyingEntity != null)
        {
            EntityMaster entity = clickedTile.occupyingEntity;

            if (entity.Faction == Faction.PLAYER && !entity.HasMoved && IsPlayerTurn)
            {
                SelectedEntity = entity;
                clickedTile.ShowMoveAreaBFS(entity.MoveRange);
                ClearAllMoveAreas();
                return;
            }
        }

        // Case 2: clicking a tile that is part of a move area
        if (clickedTile._isMoveArea && SelectedEntity != null)
        {
            SelectedEntity.StartCoroutine(
                SelectedEntity.MoveToGridPosition(clickedTile.gridX, clickedTile.gridZ)
            );

            SelectedEntity = null;
        }
        ClearAllMoveAreas();
    }


    public void ClearAllMoveAreas()
    {
        GridManager grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        foreach (Tile t in grid.GetAllTiles())
        {
            t.ClearMoveArea();
        }
    }


    public void RefreshTileList()
    {
        allTiles = FindObjectsOfType<Tile>();
    }
    
    public void EndTurn()
    {
        IsPlayerTurn = !IsPlayerTurn;

        SelectedEntity = null;
        ClearAllMoveAreas();

        if (!IsPlayerTurn)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator<WaitForSeconds> EnemyTurnRoutine()
    {
        Debug.Log("[TurnManager] Enemy turn started...");

        yield return new WaitForSeconds(1f);

        EndTurn();
    }
}
