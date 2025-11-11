using System.Collections.Generic;
using UnityEngine;

public abstract class SummonerDeckManager : MonoBehaviour
{
    [SerializeField] private DeckData deckData;
    protected List<Card> deck;
    protected List<Card> hand = new();

    void Awake()
    {
        if (deckData != null)
            deck = new List<Card>(deckData.deck);
        else
            deck = new List<Card>();
    }

    public virtual void InitDeck()
    {
        deck = new List<Card>(deckData.deck);
        ShuffleDeck();
    }

    public virtual void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int rand = Random.Range(0, deck.Count);
            (deck[i], deck[rand]) = (deck[rand], deck[i]);
        }
    }

    public virtual Card DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogWarning($"[{name}] Deck is empty!");
            return null;
        }

        Card drawn = deck[0];
        deck.RemoveAt(0);
        hand.Add(drawn);
        OnCardDrawn(drawn);

        return drawn;
    }

    public virtual void UseCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            OnCardUsed(card);
        }
    }

    protected abstract void OnCardDrawn(Card card);
    protected abstract void OnCardUsed(Card card);
}
