using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/Card")]
public abstract class Card : ScriptableObject
{
    public abstract string CardName { get; }
    public abstract Sprite CardSprite { get; }
    public abstract Sprite WorldSprite { get; }
    public abstract GameObject Prefab { get; }
    public abstract int SummonCost { get; }
}
