using UnityEngine;

public class EntitySummon
{
    private EntityMaster _e;

    public EntitySummon(EntityMaster e)
    {
        _e = e;
    }

    public void ShowSummonArea()
    {
        if (_e.data.faction != Faction.PLAYER) return;
        if (TurnManager.GetCurrentTurn() != Faction.PLAYER) return;

        int x = _e.pos.GridX;
        int z = _e.pos.GridZ;

        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[EntitySummon] GridManager tidak ditemukan!");
            return;
        }

        Tile startTile = grid.GetTileAt(x, z);
        if (startTile == null)
        {
            Debug.LogError($"[EntitySummon] Tile di posisi ({x}, {z}) tidak ditemukan!");
            return;
        }

        PlayerManager.Instance.ClearAllMoveAreas();

        int summonRange = _e.data.summonRange;
        startTile.tileAction.ShowActionAreaBFS(summonRange);

        Debug.Log($"[EntitySummon] Menampilkan area summon dari tile ({x}, {z}) dengan jangkauan {summonRange}.");
    }

    public void HideSummonArea()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[EntitySummon] GridManager tidak ditemukan!");
            return;
        }

        foreach (Tile tile in grid.GetAllTiles())
        {
            tile.tileAction.ClearActionArea();
        }

        Debug.Log("[EntitySummon] Semua area summon disembunyikan.");
    }

}
