using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityData", menuName = "Entities/Entity Data")]
public class EntityData : ScriptableObject
{
    public string entityName;
    public GameObject prefab;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int attackRange;
    public int moveRange;
    public int critChance;
    public Faction faction;

    public void setCurrentHP(int hp)
    {
        this.currentHP = hp;
    }
}
