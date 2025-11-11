using UnityEngine;

public class CardManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Player deck manager — sumber data kartu pemain")]
    public PlayerDeckManager playerDeckManager;

    [Tooltip("Tempat menampilkan kartu di tangan (biasanya CardContainer)")]
    public Transform cardContainer;

    [Tooltip("Prefab kartu yang memiliki script CardDisplay")]
    public GameObject cardPrefab;

    private void Start()
    {
        if (playerDeckManager == null)
        {
            Debug.LogError("[CardManager] PlayerDeckManager belum di-assign!");
            return;
        }

        LoadHandFromDeck();
    }

    public void LoadHandFromDeck()
    {
        // Bersihkan tampilan lama
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Pastikan deck tidak kosong
        if (playerDeckManager.Hand == null || playerDeckManager.Hand.Count == 0)
        {
            Debug.LogWarning("[CardManager] Tidak ada kartu di tangan PlayerDeckManager!");
            return;
        }

        // Tampilkan semua kartu dari tangan
        foreach (Card card in playerDeckManager.Hand)
        {
            if (card == null) continue;

            GameObject newCard = Instantiate(cardPrefab, cardContainer);
            CardDisplay display = newCard.GetComponent<CardDisplay>();

            if (display != null)
                display.LoadCard(card);
            else
                Debug.LogWarning("[CardManager] CardPrefab tidak punya komponen CardDisplay!");
        }
    }

    public void RefreshHand()
    {
        LoadHandFromDeck();
    }
}
