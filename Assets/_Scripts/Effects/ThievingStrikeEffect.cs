using System.Collections.Generic;
using UnityEngine;

public class ThievingStrikeEffect : Effect
{
    [SerializeField] private int damage;
    [SerializeField] private int goldAmount;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new ThievingStrikeGA(damage, goldAmount, targets);
    }
}
