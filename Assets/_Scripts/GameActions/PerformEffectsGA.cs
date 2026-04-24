using System.Collections.Generic;

public class PerformEffectsGA : GameAction
{
    public Effect Effect { get; private set; }
    public List<CombatantView> Targets { get; private set; }
    public CombatantView Caster { get; private set; }

    public PerformEffectsGA(Effect effect, List<CombatantView> targets, CombatantView caster = null)
    {
        Effect = effect;
        Targets = targets;
        Caster = caster;
    }
}