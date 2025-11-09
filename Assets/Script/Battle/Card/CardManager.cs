using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("Available Cards")]
    public Card[] allCards;

    [Header("Hand Settings")]
    [Tooltip("Drag your 'card-container' object here")]
    public Transform cardContainer;

    [Tooltip("Prefab that has CardDisplay script")]
    public GameObject cardPrefab;

    private void Start()
    {
        LoadHand();
    }

    public void LoadHand()
    {
        // Clear old cards
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new ones
        foreach (Card card in allCards)
        {
            if (card == null) continue;

            GameObject newCard = Instantiate(cardPrefab, cardContainer);
            CardDisplay display = newCard.GetComponent<CardDisplay>();
            display.LoadCard(card);
        }
    }
}
