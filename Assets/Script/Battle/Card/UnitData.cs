using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : Card
{
    [Header("Card Info")]
    public string unitName;

    [Header("Stats")]
    public int attack;
    public int health;
    public int crit;
    public int moveRange;
    public int attackRange;
    public int summonRange;
    public int summonCost;

    [Header("Visual")]
    public Sprite cardSprite;
    public Sprite worldSprite;
    public GameObject prefab;

    // Implement base properties
    public override string CardName => unitName;
    public override Sprite CardSprite => cardSprite;
    public override Sprite WorldSprite => worldSprite;
    public override GameObject Prefab => prefab;
    public override int SummonCost => summonCost;
}
