public class AddManaGA : GameAction
{
    public int Amount { get; private set; }

    public AddManaGA(int amount)
    {
        Amount = amount;
    }
}
