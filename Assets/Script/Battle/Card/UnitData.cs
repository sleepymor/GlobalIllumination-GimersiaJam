using UnityEngine;

[CreateAssetMenu(fileName = "NewEntityData", menuName = "Entities/Entity Data")]
public class EntityData : ScriptableObject
{
    public string entityName;
    public GameObject prefab;
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int attackRange;
    public int summonRange = 1;
    public int moveRange;
    public int critChance;
    public Faction faction;
    public bool canSummon;
}

public enum Faction
{
    PLAYER,
    ALLY,
    ENEMY,
    WILD
}

