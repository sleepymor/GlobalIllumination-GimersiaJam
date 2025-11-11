using UnityEngine;
using System.Collections.Generic;

public class PlayerDeckManager : SummonerDeckManager
{
    [Header("UI References")]
    [SerializeField] private CardContainer handContainer;
    [SerializeField] private GameObject cardPrefab;
    public List<Card> Hand => hand;

    public static PlayerDeckManager Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected override void OnCardDrawn(Card card)
    {
        if (handContainer == null)
        {
            Debug.LogWarning("[PlayerDeck] No CardContainer assigned!");
            return;
        }

        handContainer.AddCard(card, cardPrefab);
        Debug.Log($"[PlayerDeck] Drew card: {card.name}");
    }

    protected override void OnCardUsed(Card card)
    {
    }

    public void DrawMultiple(int count)
    {
        for (int i = 0; i < count; i++)
            DrawCard();
    }

    public void ClearHand()
    {
        // handContainer.ClearCards();
        // hand.Clear();
    }
}
