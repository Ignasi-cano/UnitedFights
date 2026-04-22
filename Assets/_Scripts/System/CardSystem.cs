using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using DG.Tweening;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;
    [SerializeField] private List<CardData> availableCards;
    public List<CardData> AvailableCards => availableCards;

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();

    void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
        ActionSystem.AttachPerformer<DiscardCardGA>(DiscardCardPerformer);
        ActionSystem.AttachPerformer<DiscardRandomCardGA>(DiscardRandomCardPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
        ActionSystem.DetachPerformer<DiscardCardGA>();
        ActionSystem.DetachPerformer<DiscardRandomCardGA>();
    }

    public void Setup(List<CardData> deckData)
    {
        foreach (var cardData in deckData)
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }

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

    private IEnumerator DiscardCardPerformer(DiscardCardGA action)
    {
        if (hand.Contains(action.Card))
        {
            CardView cv = handView.RemoveCard(action.Card);
            hand.Remove(action.Card);
            if (cv != null) yield return DiscardCard(cv);
        }
    }

    private IEnumerator DiscardRandomCardPerformer(DiscardRandomCardGA action)
    {
        if (hand.Count > 0)
        {
            Card randomCard = hand[Random.Range(0, hand.Count)];
            CardView cv = handView.RemoveCard(randomCard);
            hand.Remove(randomCard);
            if (cv != null) yield return DiscardCard(cv);
        }
    }

    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        if (card == null) yield break;

        hand.Add(card);
        Vector3 spawnPos = drawPilePoint != null ? drawPilePoint.position : Vector3.zero;
        Quaternion spawnRot = drawPilePoint != null ? drawPilePoint.rotation : Quaternion.identity;
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, spawnPos, spawnRot);
        
        ActionSystem.Instance.AddReaction(new CardDrawnGA(card));

        if (card.Data.PassiveEffects != null)
        {
            foreach (var passive in card.Data.PassiveEffects)
            {
                passive.OnDraw(card);
            }
        }

        yield return handView.AddCard(cardView);
    }
    private void RefillDeck()
    {
       drawPile.AddRange(discardPile);
       discardPile.Clear(); 
    }
    private IEnumerator DiscardCard(CardView cardView)
    {
        if (cardView == null) yield break;

        discardPile.Add(cardView.Card);
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        
        Vector3 targetPos = discardPilePoint != null ? discardPilePoint.position : cardView.transform.position;
        Tween tween = cardView.transform.DOMove(targetPos, 0.15f);
        yield return tween.WaitForCompletion();
        
        if (cardView != null && cardView.gameObject != null)
            Destroy(cardView.gameObject);
    }
    public void AddCardToDeck(CardData cardData)
    {
        Card card = new(cardData);
        drawPile.Add(card);
        Debug.Log(card + " added to the deck");
    }
    public List<CardData> GetDeckData()
    {
        List<CardData> deck = new();
        foreach (var card in drawPile) deck.Add(card.Data);
        foreach (var card in discardPile) deck.Add(card.Data);
        return deck;
    }
    public void RemoveCard(CardData cardData)
    {
        Card toRemove = drawPile.Find(c => c.Data == cardData);
        if (toRemove != null)
        {
            drawPile.Remove(toRemove);
            Debug.Log($"[CardSystem] Removed {cardData.name} from draw pile.");
            return;
        }

        // Then check discard pile
        toRemove = discardPile.Find(c => c.Data == cardData);
        if (toRemove != null)
        {
            discardPile.Remove(toRemove);
            Debug.Log($"[CardSystem] Removed {cardData.name} from discard pile.");
        }
    }
    public int GetTotalHandSizeModifier()
    {
        int total = 0;
        List<CardData> deck = GetDeckData();
        foreach (var cardData in deck)
        {
            if (cardData.PassiveEffects == null) continue;
            foreach (var passive in cardData.PassiveEffects)
            {
                total += passive.GetHandSizeModifier();
            }
        }
        return total;
    }
}
