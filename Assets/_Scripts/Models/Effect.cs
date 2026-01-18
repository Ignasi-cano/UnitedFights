using System.Collections.Generic;

[System.Serializable]
public abstract class Effect 
{
    public abstract GameAction GetGameAction(List<CombatantView> targets, CombatantView caster);

    public virtual void ApplyToInstances(List<HeroInstance> targets)
    {
        // Default: Do nothing, override in specific effects like ChangeMaxHealth
    }
}
