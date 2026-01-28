public class CardDrawnGA : GameAction
{
    public Card Card { get; }
    public CardDrawnGA(Card card) => Card = card;
}
