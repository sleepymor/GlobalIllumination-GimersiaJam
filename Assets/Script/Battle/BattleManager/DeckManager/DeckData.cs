using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "DeckData", menuName = "Scriptable Objects/DeckData")]
public class DeckData : ScriptableObject
{
    [SerializeField] public List<Card> deck = new();
}
