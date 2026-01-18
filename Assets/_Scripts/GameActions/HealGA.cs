using System.Collections.Generic;

public class HealGA : GameAction
{
    public List<CombatantView> Targets { get; private set; }
    public int Amount { get; private set; }

    public HealGA(List<CombatantView> targets, int amount)
    {
        Targets = targets;
        Amount = amount;
    }
}
