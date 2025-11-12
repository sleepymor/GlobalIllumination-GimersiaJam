using UnityEngine;

public class EntityItem
{
    private EntityMaster _e;

    public EntityItem(EntityMaster e)
    {
        _e = e;
    }

    public void ShowEquipArea()
    {
        if (_e.data.faction != Faction.PLAYER) return;
        if (TurnManager.GetCurrentTurn() != Faction.PLAYER) return;

        int x = _e.pos.GridX;
        int z = _e.pos.GridZ;

        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[EntityItem] GridManager tidak ditemukan!");
            return;
        }

        Tile startTile = grid.GetTileAt(x, z);
        if (startTile == null)
        {
            Debug.LogError($"[EntityItem] Tile di posisi ({x}, {z}) tidak ditemukan!");
            return;
        }

        PlayerManager.Instance.ClearAllMoveAreas();

        startTile.tileAction.ShowActionAreaBFS(1, true);

        Debug.Log($"[EntityItem] Menampilkan area equip");
    }

    public void HideEquipArea()
    {
        GridManager grid = GridManager.Instance;
        if (grid == null)
        {
            Debug.LogError("[EntityItem] GridManager tidak ditemukan!");
            return;
        }

        foreach (Tile tile in grid.GetAllTiles())
        {
            tile.tileAction.ClearActionArea();
        }

        Debug.Log("[EntityItem] Semua area equip disembunyikan.");
    }
    
}
