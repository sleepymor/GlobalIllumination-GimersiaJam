using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [Header("Card Data (Unit or Spell)")]
    public Card cardData;

    [Header("UI References")]
    public Image cardImage;

    public void LoadCard(Card data)
    {
        cardData = data;

        if (data == null) return;

        // Display visuals
        if (cardImage != null && data.CardSprite != null)
            cardImage.sprite = data.CardSprite;
    }

    public void OnPlayCard(Vector3 spawnPos)
    {
        if (cardData == null) return;

        if (cardData.Prefab != null)
        {
            Instantiate(cardData.Prefab, spawnPos, Quaternion.identity);
        }
        else if (cardData.WorldSprite != null)
        {
            GameObject worldObj = new GameObject(cardData.CardName);
            SpriteRenderer sr = worldObj.AddComponent<SpriteRenderer>();
            sr.sprite = cardData.WorldSprite;
            worldObj.transform.position = spawnPos;
        }
    }

    void Start()
    {
        if (cardData != null)
            LoadCard(cardData);
    }


}


