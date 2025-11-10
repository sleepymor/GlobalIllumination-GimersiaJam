using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : Card
{
    [Header("Card Info")]
    public string unitName;

    [Header("Stats")]
    public int health;
    public int attack;
    [HideInInspector] public int currentHP;
    public int critDmg;
    public int defense;
    public int moveRange;
    public int attackRange;
    public int critChance;
    public int summonCost;


    [Header("Summoner stat")]
    public bool canSummon;
    public int summonRange;
    public int maxSoul;
    public int soulRecovery;
    [HideInInspector] public int currentSoul;


    public Faction faction;

    [Header("Visual")]
    public Sprite cardSprite;
    public Sprite worldSprite;
    public GameObject prefab;
    public override string CardName => unitName;
    public override Sprite CardSprite => cardSprite;
    public override Sprite WorldSprite => worldSprite;
    public override GameObject Prefab => prefab;
    public override int SummonCost => summonCost;
}

public enum Faction
{
    PLAYER,
    ALLY,
    ENEMY,
    WILD
}
