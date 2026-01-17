using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using DG.Tweening;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private List<CardData> availableCards; // Lista de todas las cartas disponibles para elegir random

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand =new();

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();

    }
    //Publics

    public void Setup(List<CardData> deckData)
    {
        foreach (var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }

    //performers

    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;

        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }
        if(notDrawnAmount > 0)
        {
            RefillDeck();
            int remainingToDraw = Mathf.Min(notDrawnAmount, drawPile.Count);
            for(int i = 0; i < remainingToDraw; i++)
            {
                yield return DrawCard();
            }
        }
    }
    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach(var card in hand)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    }
    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardView);

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);
        
        if(playCardGA.Card.ManualTargetEffect !=null)
        {
            PerformEffectsGA performEffectsGA = new(playCardGA.Card.ManualTargetEffect,new(){ playCardGA.ManualTarget });
            ActionSystem.Instance.AddReaction(performEffectsGA);
        }
        foreach (var effectWrapper in playCardGA.Card.OtherEffects)
        {
            List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
            PerformEffectsGA performEffectGA = new(effectWrapper.Effect, targets);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
    }

    //helpers
    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        yield return handView.AddCard(cardView);
    }
    private void RefillDeck()
    {
       drawPile.AddRange(discardPile);
       discardPile.Clear(); 
    }
    private IEnumerator DiscardCard(CardView cardView)
    {
        discardPile.Add(cardView.Card);
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }

    // Método para añadir una carta al mazo del héroe
    public void AddCardToDeck(CardData cardData)
    {
        Card card = new(cardData);
        drawPile.Add(card);
        Debug.Log(card + " added to the deck");
    }

    // Método para añadir una carta random al mazo del héroe
    public void AddRandomCardToDeck()
    {
        if (availableCards != null && availableCards.Count > 0)
        {
            CardData randomCardData = availableCards[Random.Range(0, availableCards.Count)];
            AddCardToDeck(randomCardData);
        }
        else
        {
            Debug.LogWarning("No available cards to add randomly.");
        }
    }
}
