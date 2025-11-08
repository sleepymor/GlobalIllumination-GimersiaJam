using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public GameObject prefab;
    public BuffType BuffType;

    public int amount;
}

public enum BuffType
{
    Heal,
    Attack,
    Defense,
    Crit
}

