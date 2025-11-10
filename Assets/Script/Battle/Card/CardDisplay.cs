using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [Header("Card Data (Unit or Spell)")]
    public Card cardData;

    [Header("UI References")]
    public Image cardImage;

    public TMP_Text Cost;
    public TMP_Text Attack;
    public TMP_Text HealthPoint;
    public TMP_Text MiscPoint;


    public void LoadCard(Card data)
    {
        cardData = data;

        if (data == null) return;

        // Display visuals
        if (cardImage != null && data.CardSprite != null)
            cardImage.sprite = data.CardSprite;

        Cost.text = $"{data.SummonCost}";

        if (data is UnitData unit)
        {
            Attack.text = $"{unit.attack}";
            HealthPoint.text = $"{unit.health}";
        }
        else if (data is SpellData spell)
        {
            MiscPoint.text = $"{spell.amount}";
        }
        else if (data is ItemData item)
        {
            MiscPoint.text = $"{item.amount}";
        }

        // Show extra info depending on card type
        // if (data is UnitData unit)
        // {
        //     extraText.text = $"ATK: {unit.attack} | HP: {unit.health}";
        // }
        // else if (data is SpellData spell)
        // {
        //     extraText.text = $"{spell.DamageType} {spell.amount} dmg";
        // }
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


