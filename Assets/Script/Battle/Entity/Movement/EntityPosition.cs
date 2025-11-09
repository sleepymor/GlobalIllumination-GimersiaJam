using UnityEngine;
public class EntityPosition
{
    [Header("Entity Grid Position")]
    public int gridX;
    public int gridZ;
    public float heightAboveTile = 3f;
    public int GridX => gridX;
    public int GridZ => gridZ;

    public EntityPosition(int x, int z)
    {
        this.gridX = x;
        this.gridZ = z;
    }

    public void SetPos(int x, int z)
    {
        gridX = x;
        gridZ = z;
    }
}