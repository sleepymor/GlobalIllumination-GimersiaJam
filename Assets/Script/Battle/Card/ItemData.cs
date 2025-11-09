using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : Card
{
    [Header("Card Info")]
    public string itemName;
    
    [Header("Stats")]
    public BuffType BuffType;
    public int amount;
    public int summonCost;

    [Header("Visual")]
    public Sprite cardSprite;
    public Sprite worldSprite;
    public GameObject prefab;

    public override string CardName => itemName;
    public override Sprite CardSprite => cardSprite;
    public override Sprite WorldSprite => worldSprite;
    public override GameObject Prefab => prefab;
    public override int SummonCost => summonCost;
}

public enum BuffType
{
    Heal,
    Attack,
    Defense,
    Crit
}

