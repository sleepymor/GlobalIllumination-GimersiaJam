using UnityEngine;

[CreateAssetMenu(fileName = "SpellData", menuName = "Scriptable Objects/SpellData")]
public class SpellData : ScriptableObject
{
    public string spellName;

    public GameObject prefab;

    public DamageType DamageType;

    public int amount;
}

public enum DamageType
{
    DOT,
    AOE,
    Freeze,
}
