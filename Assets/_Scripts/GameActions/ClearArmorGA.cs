using System.Collections.Generic;

public class ClearArmorGA : GameAction, IHaveTargets
{
    public List<CombatantView> Targets { get; }

    public ClearArmorGA(List<CombatantView> targets)
    {
        Targets = targets;
    }
}
