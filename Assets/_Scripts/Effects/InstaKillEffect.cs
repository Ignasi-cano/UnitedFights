using System.Collections.Generic;
using UnityEngine;

public class InstaKillEffect : Effect
{
    [SerializeField] private int massiveDamage = 99999;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Dealing massive damage is the most compatible way with current DamageSystem
        return new DealDamageGA(massiveDamage, targets, caster);
    }
}
