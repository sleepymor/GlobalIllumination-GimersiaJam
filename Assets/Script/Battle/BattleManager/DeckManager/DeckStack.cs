using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representasi dari sebuah tumpukan (deck) kartu dalam game.
/// Menyimpan, menambahkan, menghapus, dan mengacak kartu.
/// </summary>
public class DeckStack
{
    private List<Card> _cards = new List<Card>();

    /// <summary>
    /// Jumlah kartu saat ini di dalam deck.
    /// </summary>
    public int Count => _cards.Count;

    /// <summary>
    /// Menambahkan kartu ke dalam deck.
    /// </summary>
    public void AddCard(Card card)
    {
        if (card == null)
        {
            Debug.LogWarning("⚠️ Tidak bisa menambahkan kartu null ke deck!");
            return;
        }

        _cards.Add(card);
    }

    /// <summary>
    /// Menghapus dan mengembalikan kartu paling atas dari deck (seperti draw).
    /// </summary>
    public Card DrawCard()
    {
        if (_cards.Count == 0)
        {
            Debug.LogWarning("⚠️ Deck kosong! Tidak ada kartu untuk di-draw.");
            return null;
        }

        Card topCard = _cards[0];
        _cards.RemoveAt(0);
        return topCard;
    }

    /// <summary>
    /// Mengembalikan kartu teratas tanpa menghapusnya.
    /// </summary>
    public Card Peek()
    {
        if (_cards.Count == 0) return null;
        return _cards[0];
    }

    /// <summary>
    /// Mengacak urutan kartu di dalam deck.
    /// </summary>
    public void Shuffle()
    {
        for (int i = 0; i < _cards.Count; i++)
        {
            int randomIndex = Random.Range(i, _cards.Count);
            ( _cards[i], _cards[randomIndex] ) = ( _cards[randomIndex], _cards[i] );
        }
    }

    /// <summary>
    /// Menghapus semua kartu dari deck.
    /// </summary>
    public void Clear()
    {
        _cards.Clear();
    }

    /// <summary>
    /// Menampilkan isi deck di console (debug).
    /// </summary>
    public void PrintDeck()
    {
        Debug.Log("🃏 Isi Deck:");
        foreach (var card in _cards)
        {
            Debug.Log($"- {card.name}");
        }
    }
}
