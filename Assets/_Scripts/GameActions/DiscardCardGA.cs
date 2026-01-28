public class DiscardCardGA : GameAction
{
    public Card Card { get; }
    public DiscardCardGA(Card card) => Card = card;
}

public class DiscardRandomCardGA : GameAction
{
    public DiscardRandomCardGA() { }
}
