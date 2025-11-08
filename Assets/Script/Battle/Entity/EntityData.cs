using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityData", menuName = "Entities/Entity Data")]
public class EntityData : ScriptableObject
{
    public string entityName;
    public GameObject prefab;
    public int maxHP;
    public int attack;
    public int moveRange;
    public int critChance;
    public Faction faction;
}
