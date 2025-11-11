using UnityEngine;
using System.Collections.Generic;

public class EnemyDeckManager : SummonerDeckManager
{
    public static EnemyDeckManager Instance;

    /// <summary>
    /// Daftar kartu yang sedang dipegang musuh (secara data saja, tidak ditampilkan).
    /// </summary>
    public List<Card> Hand => hand;

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
        Debug.Log($"[EnemyDeck] Enemy drew card: {card.name} (hidden)");
        // Data kartu tetap masuk ke hand dari SummonerDeckManager
        // Tidak perlu menampilkan ke UI
    }

    protected override void OnCardUsed(Card card)
    {
        Debug.Log($"[EnemyDeck] Enemy used card: {card.name}");
        // Kartu akan dihapus dari hand di SummonerDeckManager
    }

    /// <summary>
    /// Menarik beberapa kartu sekaligus untuk musuh.
    /// </summary>
    public void DrawMultiple(int count)
    {
        for (int i = 0; i < count; i++)
            DrawCard();
    }

    /// <summary>
    /// Menghapus semua kartu di tangan musuh.
    /// </summary>
    public void ClearHand()
    {
        hand.Clear();
        Debug.Log("[EnemyDeck] Cleared all cards from enemy hand.");
    }

    /// <summary>
    /// Memainkan satu kartu secara otomatis (contoh untuk AI).
    /// </summary>
    public void AutoPlay()
    {
        if (hand.Count == 0)
            DrawCard();

        if (hand.Count > 0)
        {
            var cardToPlay = hand[0];
            UseCard(cardToPlay);
            Debug.Log($"[EnemyDeck] Enemy auto-played card: {cardToPlay.name}");
            // EnemyAIController.Instance?.PlayCard(cardToPlay);
        }
    }
}
