using System.Collections.Generic;
using System.Collections;
using System.Linq;
using config;
using DefaultNamespace;
using events;
using UnityEngine;
using UnityEngine.UI;

public class CardContainer : MonoBehaviour
{
    [Header("Constraints")]
    [SerializeField]
    private bool forceFitContainer;

    [SerializeField]
    private bool preventCardInteraction;

    [Header("Alignment")]
    [SerializeField]
    private CardAlignment alignment = CardAlignment.Center;

    [SerializeField]
    private bool allowCardRepositioning = true;

    [Header("Rotation")]
    [SerializeField]
    [Range(-90f, 90f)]
    private float maxCardRotation;

    [SerializeField]
    private float maxHeightDisplacement;

    [SerializeField]
    private ZoomConfig zoomConfig;

    [SerializeField]
    private AnimationSpeedConfig animationSpeedConfig;

    [SerializeField]
    private CardPlayConfig cardPlayConfig;

    [Header("Events")]
    [SerializeField]
    private EventsConfig eventsConfig;

    private List<CardWrapper> cards = new();

    private RectTransform rectTransform;
    private CardWrapper currentDraggedCard;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        InitCards();
    }


    // Tambahkan ini di dalam class CardContainer
    public CardWrapper AddCard(Card cardData, GameObject cardPrefab)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[CardContainer] Card prefab tidak diset!");
            return null;
        }

        // Spawn kartu sebagai child tapi biar posisinya bisa bebas
        GameObject newCardObj = Instantiate(cardPrefab, transform, false);
        newCardObj.name = cardData.name;

        // Pastikan RectTransform dan wrapper ada
        RectTransform rect = newCardObj.GetComponent<RectTransform>();
        CardWrapper wrapper = newCardObj.GetComponent<CardWrapper>() ?? newCardObj.AddComponent<CardWrapper>();
        CardDisplay display = newCardObj.GetComponent<CardDisplay>() ?? newCardObj.AddComponent<CardDisplay>();

        // Assign data
        display.cardData = cardData;

        // Setup konfigurasi wrapper
        AddOtherComponentsIfNeeded(wrapper);
        wrapper.zoomConfig = zoomConfig;
        wrapper.animationSpeedConfig = animationSpeedConfig;
        wrapper.eventsConfig = eventsConfig;
        wrapper.preventCardInteraction = preventCardInteraction;
        wrapper.container = this;

        // --- 💥 Spawn di luar layar kanan dulu ---
        Vector3 startPos = new Vector3(Screen.width + 500f, transform.position.y, 0); // kanan luar layar
        rect.position = startPos;

        // Masukkan ke daftar kartu
        cards.Add(wrapper);

        // Jalankan animasi masuk ke tangan
        StartCoroutine(AnimateCardEntry(wrapper));

        return wrapper;
    }

    private IEnumerator AnimateCardEntry(CardWrapper wrapper)
    {
        yield return null; // tunggu 1 frame biar layout container siap

        SetCardsAnchor();
        SetCardsPosition();
        SetCardsRotation();
        SetCardsUILayers();

        // posisi target akhir (sesuai layout)
        Vector3 targetPos = wrapper.targetPosition;

        // kita ambil posisi dunia target
        Vector3 worldTarget = new Vector3(targetPos.x, targetPos.y, 0);

        float duration = 0.5f; // durasi animasi
        float elapsed = 0f;

        Vector3 startPos = wrapper.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // pakai ease-out cubic
            t = 1f - Mathf.Pow(1f - t, 3);
            wrapper.transform.position = Vector3.Lerp(startPos, worldTarget, t);
            yield return null;
        }

        wrapper.transform.position = worldTarget;
    }

    private void InitCards()
    {
        SetUpCards();
        SetCardsAnchor();
    }

    private void SetCardsRotation()
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].targetRotation = GetCardRotation(i);
            cards[i].targetVerticalDisplacement = GetCardVerticalDisplacement(i);
        }
    }

    private float GetCardVerticalDisplacement(int index)
    {
        if (cards.Count < 3) return 0;
        // Associate a vertical displacement based on the index in the cards list
        // so that the center card is at max displacement while the edges are at 0 displacement
        return maxHeightDisplacement *
               (1 - Mathf.Pow(index - (cards.Count - 1) / 2f, 2) / Mathf.Pow((cards.Count - 1) / 2f, 2));
    }

    private float GetCardRotation(int index)
    {
        if (cards.Count < 3) return 0;
        // Associate a rotation based on the index in the cards list
        // so that the first and last cards are at max rotation, mirrored around the center
        return -maxCardRotation * (index - (cards.Count - 1) / 2f) / ((cards.Count - 1) / 2f);
    }

    void Update()
    {
        UpdateCards();
    }

    void SetUpCards()
    {
        cards.Clear();
        foreach (Transform card in transform)
        {
            var wrapper = card.GetComponent<CardWrapper>();
            if (wrapper == null)
            {
                wrapper = card.gameObject.AddComponent<CardWrapper>();
            }

            cards.Add(wrapper);

            AddOtherComponentsIfNeeded(wrapper);

            // Pass child card any extra config it should be aware of
            wrapper.zoomConfig = zoomConfig;
            wrapper.animationSpeedConfig = animationSpeedConfig;
            wrapper.eventsConfig = eventsConfig;
            wrapper.preventCardInteraction = preventCardInteraction;
            wrapper.container = this;
        }
    }

    private void AddOtherComponentsIfNeeded(CardWrapper wrapper)
    {
        var canvas = wrapper.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = wrapper.gameObject.AddComponent<Canvas>();
        }

        canvas.overrideSorting = true;

        if (wrapper.GetComponent<GraphicRaycaster>() == null)
        {
            wrapper.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void UpdateCards()
    {
        if (transform.childCount != cards.Count)
        {
            InitCards();
        }

        if (cards.Count == 0)
        {
            return;
        }

        SetCardsPosition();
        SetCardsRotation();
        SetCardsUILayers();
        UpdateCardOrder();
    }

    private void SetCardsUILayers()
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].uiLayer = zoomConfig.defaultSortOrder + i;
        }
    }

    private void UpdateCardOrder()
    {
        if (!allowCardRepositioning || currentDraggedCard == null) return;

        // Get the index of the dragged card depending on its position
        var newCardIdx = cards.Count(card => currentDraggedCard.transform.position.x > card.transform.position.x);
        var originalCardIdx = cards.IndexOf(currentDraggedCard);
        if (newCardIdx != originalCardIdx)
        {
            cards.RemoveAt(originalCardIdx);
            if (newCardIdx > originalCardIdx && newCardIdx < cards.Count - 1)
            {
                newCardIdx--;
            }

            cards.Insert(newCardIdx, currentDraggedCard);
        }
        // Also reorder in the hierarchy
        currentDraggedCard.transform.SetSiblingIndex(newCardIdx);
    }

    private void SetCardsPosition()
    {
        // Compute the total width of all the cards in global space
        var cardsTotalWidth = cards.Sum(card => card.width * card.transform.lossyScale.x);
        // Compute the width of the container in global space
        var containerWidth = rectTransform.rect.width * transform.lossyScale.x;
        if (forceFitContainer && cardsTotalWidth > containerWidth)
        {
            DistributeChildrenToFitContainer(cardsTotalWidth);
        }
        else
        {
            DistributeChildrenWithoutOverlap(cardsTotalWidth);
        }
    }

    private void DistributeChildrenToFitContainer(float childrenTotalWidth)
    {
        // Get the width of the container
        var width = rectTransform.rect.width * transform.lossyScale.x;
        // Get the distance between each child
        var distanceBetweenChildren = (width - childrenTotalWidth) / (cards.Count - 1);
        // Set all children's positions to be evenly spaced out
        var currentX = transform.position.x - width / 2;
        foreach (CardWrapper child in cards)
        {
            var adjustedChildWidth = child.width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentX + adjustedChildWidth / 2, transform.position.y);
            currentX += adjustedChildWidth + distanceBetweenChildren;
        }
    }

    private void DistributeChildrenWithoutOverlap(float childrenTotalWidth)
    {
        var currentPosition = GetAnchorPositionByAlignment(childrenTotalWidth);
        foreach (CardWrapper child in cards)
        {
            var adjustedChildWidth = child.width * child.transform.lossyScale.x;
            child.targetPosition = new Vector2(currentPosition + adjustedChildWidth / 2, transform.position.y);
            currentPosition += adjustedChildWidth;
        }
    }

    private float GetAnchorPositionByAlignment(float childrenWidth)
    {
        var containerWidthInGlobalSpace = rectTransform.rect.width * transform.lossyScale.x;
        switch (alignment)
        {
            case CardAlignment.Left:
                return transform.position.x - containerWidthInGlobalSpace / 2;
            case CardAlignment.Center:
                return transform.position.x - childrenWidth / 2;
            case CardAlignment.Right:
                return transform.position.x + containerWidthInGlobalSpace / 2 - childrenWidth;
            default:
                return 0;
        }
    }

    private void SetCardsAnchor()
    {
        foreach (CardWrapper child in cards)
        {
            child.SetAnchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f));
        }
    }

    public void OnCardDragStart(CardWrapper card)
    {
        currentDraggedCard = card;

        CardDisplay cardDisplay = card.GetComponent<CardDisplay>();
        if (cardDisplay == null || cardDisplay.cardData == null)
        {
            Debug.LogWarning("[CardContainer] CardDisplay or cardData not found!");
            currentDraggedCard = null;
            return;
        }

        Card data = cardDisplay.cardData;


        if (cardDisplay == null && cardDisplay.cardData == null)
        {
            Debug.LogWarning("[CardContainer] UnitCard atau EntityData tidak ditemukan!");
            return;
        }

        EntityMaster summoner = PlayerManager.Instance.GetSummoner();
        if (data is UnitData unitData)
        {
            SummonManager.Instance.ShowSummonArea(summoner, cardDisplay.cardData);
            return;
        }

        if (data is ItemData itemData)
        {
            List<EntityMaster> unit = PlayerManager.Instance.TeamList;
            ItemManager.Instance.ShowEquipArea(summoner, unit, cardDisplay.cardData);
            return;
        }

        if (data is SpellData spellData)
        {
            List<EntityMaster> unit = EnemyManager.Instance.TeamList;
            SpellManager.Instance.ShowActionArea(summoner, unit, cardDisplay.cardData);
            return;
        }

    }

    public void OnCardDragEnd()
    {
        if (currentDraggedCard == null) return;

        CardDisplay cardDisplay = currentDraggedCard.GetComponent<CardDisplay>();

        if (cardDisplay == null || cardDisplay.cardData == null)
        {
            Debug.LogWarning("[CardContainer] CardDisplay or cardData not found!");
            currentDraggedCard = null;
            return;
        }

        Card data = cardDisplay.cardData;

        if (data is UnitData unitData)
        {
            SummonManager.Instance.ShowSummonArea(PlayerManager.Instance.GetSummoner(), unitData);
            SummonManager.Instance.SummonAtTile(currentDraggedCard);
        }
        else if (data is SpellData spellData)
        {
            SpellManager.Instance.ShowActionArea(PlayerManager.Instance.GetSummoner(), EnemyManager.Instance.TeamList, spellData);
            SpellManager.Instance.ActivateAt(currentDraggedCard);
        }
        else if (data is ItemData itemData)
        {
            ItemManager.Instance.ShowEquipArea(PlayerManager.Instance.GetSummoner(), PlayerManager.Instance.TeamList, itemData);
            ItemManager.Instance.EquipAt(currentDraggedCard);
        }
        else
        {
            Debug.LogWarning("[CardContainer] Unknown card type!");
            Destroy(currentDraggedCard.gameObject);
        }

        currentDraggedCard = null;
    }


    public void DestroyCard(CardWrapper card)
    {
        cards.Remove(card);
        eventsConfig.OnCardDestroy?.Invoke(new CardDestroy(card));
        Destroy(card.gameObject);
    }

    private bool IsCursorInPlayArea()
    {
        if (cardPlayConfig.playArea == null) return false;

        var cursorPosition = Input.mousePosition;
        var playArea = cardPlayConfig.playArea;
        var playAreaCorners = new Vector3[4];
        playArea.GetWorldCorners(playAreaCorners);
        return cursorPosition.x > playAreaCorners[0].x &&
               cursorPosition.x < playAreaCorners[2].x &&
               cursorPosition.y > playAreaCorners[0].y &&
               cursorPosition.y < playAreaCorners[2].y;

    }
}
