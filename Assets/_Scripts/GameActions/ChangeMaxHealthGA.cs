using System.Collections.Generic;

public class ChangeMaxHealthGA : GameAction
{
    public List<CombatantView> Targets { get; private set; }
    public int Amount { get; private set; }

    public ChangeMaxHealthGA(List<CombatantView> targets, int amount)
    {
        Targets = targets;
        Amount = amount;
    }
}
