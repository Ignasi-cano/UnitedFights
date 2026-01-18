using UnityEngine;

public class GiveGoldGA : GameAction
{
    public int Amount { get; private set; }

    public GiveGoldGA(int amount)
    {
        Amount = amount;
    }
}
