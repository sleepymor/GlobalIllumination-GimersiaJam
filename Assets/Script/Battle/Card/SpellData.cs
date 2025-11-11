using UnityEngine;

[CreateAssetMenu(fileName = "SpellData", menuName = "Scriptable Objects/SpellData")]
public class SpellData : Card
{
    [Header("Card Info")]
    public string spellName;

    [Header("Stats")]
    public DamageType DamageType;
    public int amount;
    // public int spellRange;
    public int spellDuration;
    public int aoeRange;
    public int summonCost;

    [Header("Visual")]
    public Sprite cardSprite;
    public Sprite worldSprite;
    public GameObject prefab;

    
    public override string CardName => spellName;
    public override Sprite CardSprite => cardSprite;
    public override Sprite WorldSprite => worldSprite;
    public override GameObject Prefab => prefab;
    public override int SummonCost => summonCost;
}

public enum DamageType
{
    DOT,
    AOE,
    Freeze,
}
