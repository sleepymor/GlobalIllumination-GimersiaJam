using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int _width = 8;
    [SerializeField] private int _height = 8;
    [SerializeField] private float _tileSize = 10f;

    public int Width => _width;
    public int Height => _height;
    public float TileSize => _tileSize;

    [Header("References")]
    [SerializeField] private Tile _tilePrefab;

    private Tile[,] _tiles;

    private void Awake()
    {
        if (_tiles == null)
            GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (_tilePrefab == null)
        {
            Debug.LogError("Tile prefab not assigned in GridManager!");
            return;
        }

        _tiles = new Tile[_width, _height];

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                Vector3 worldPos = new Vector3(x * _tileSize, 0, z * _tileSize);
                Tile spawnedTile = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);
                spawnedTile.name = $"Tile ({x},{z})";

                bool isOffset = (x + z) % 2 == 1;
                spawnedTile.Init(isOffset);

                spawnedTile.gridX = x;
                spawnedTile.gridZ = z;

                _tiles[x, z] = spawnedTile;
            }
        }

        Debug.Log($"✅ Generated {_width}x{_height} grid on XZ plane.");
        TurnManager.Instance?.RefreshTileList();

    }

    public Tile GetTileAt(int x, int z)
    {
        if (_tiles == null)
        {
            Debug.LogWarning("[GridManager] Tiles not yet generated — generating now...");
            GenerateGrid();
        }

        if (x < 0 || x >= _width || z < 0 || z >= _height)
            return null;

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
