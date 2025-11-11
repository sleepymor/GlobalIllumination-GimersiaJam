using UnityEngine;

public class EntitySpell
{
    private EntityMaster _e;

    public EntitySpell(EntityMaster e)
    {
        _e = e;
    }

    public void ShowActionArea(SpellData data)
    {
        // if (data.DamageType == DamageType.AOE)
        // {
        //     ShowAOEArea(data);
        //     return;
        // }

        // else
        // {
            ShowAttachArea();
        // }
    }

    public void ShowAttachArea()
    {
        if (_e.data.faction != Faction.ENEMY) return;
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

    public void HideActionArea()
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

        Debug.Log("[EntityItem] Semua area summon disembunyikan.");
    }

}
