using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileRow
{
    public Tile[] row; // each row of tile prefabs
}

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private List<TileRow> tileRows;

    private Tile[,] _tiles;

    public int Width => _tiles?.GetLength(0) ?? 0;
    public int Height => _tiles?.GetLength(1) ?? 0;
    public float TileSize => tileSize;

    private void Awake()
    {
        if (_tiles == null)
            GenerateGrid();
    }

    public void GenerateGrid()
    {
        if (tileRows == null || tileRows.Count == 0)
        {
            Debug.LogError("TileRows not assigned!");
            return;
        }

        int width = tileRows[0].row.Length;
        int height = tileRows.Count;

        _tiles = new Tile[width, height];

        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Tile prefab = GetPrefabAt(x, z);
                if (prefab == null) continue;

                Vector3 worldPos = new Vector3(x * tileSize, 0, z * tileSize);
                Tile spawnedTile = Instantiate(prefab, worldPos, Quaternion.identity, transform);
                spawnedTile.name = $"Tile ({x},{z})";

                spawnedTile.gridX = x;
                spawnedTile.gridZ = z;

                _tiles[x, z] = spawnedTile;
            }
        }

        Debug.Log($"Generated {width}x{height} grid from TileRows");
    }

    private Tile GetPrefabAt(int x, int z)
    {
        if (z < 0 || z >= tileRows.Count) return null;
        if (x < 0 || x >= tileRows[z].row.Length) return null;
        return tileRows[z].row[x];
    }

    public Tile GetTileAt(int x, int z)
    {
        if (_tiles == null) return null;
        if (x < 0 || x >= Width || z < 0 || z >= Height) return null;
        return _tiles[x, z];
    }

    public Vector2Int GetTileCoordinates(Tile tile)
    {
        if (_tiles == null || tile == null) return new Vector2Int(-1, -1);
        return new Vector2Int(tile.gridX, tile.gridZ);
    }

    public List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int coords = GetTileCoordinates(tile);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in directions)
        {
            Tile neighbor = GetTileAt(coords.x + dir.x, coords.y + dir.y);
            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    public Tile[] GetAllTiles()
    {
        if (_tiles == null) return new Tile[0];
        List<Tile> tileList = new List<Tile>();
        for (int x = 0; x < Width; x++)
            for (int z = 0; z < Height; z++)
                tileList.Add(_tiles[x, z]);
        return tileList.ToArray();
    }
}
