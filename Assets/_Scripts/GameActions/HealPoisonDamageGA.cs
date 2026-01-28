using System.Collections.Generic;

public class HealPoisonDamageGA : GameAction, IHaveTargets
{
    public List<CombatantView> Targets { get; private set; }

    public HealPoisonDamageGA(CombatantView target)
    {
        Targets = new List<CombatantView> { target };
    }
}
