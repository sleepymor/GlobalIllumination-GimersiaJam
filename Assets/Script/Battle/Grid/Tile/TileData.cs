using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "Tiles/Tile Data")]
public class TileData : ScriptableObject
{
    public string tileName;

    public GameObject visualPrefab;
    public int moveCost;
    public bool isMoveArea;
    public int dmgTurn;
    public int healTurn;
    public int def;
    
}
