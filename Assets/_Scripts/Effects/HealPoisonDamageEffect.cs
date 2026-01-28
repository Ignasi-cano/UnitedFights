using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HealPoisonDamageEffect : Effect
{
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // We expect single target (the owner of the perk usually)
        if (targets.Count > 0)
        {
            Debug.Log($"[HealPoisonDamageEffect] Creating HealPoisonDamageGA for target: {targets[0].name}");
            return new HealPoisonDamageGA(targets[0]);
        }
        Debug.LogWarning("[HealPoisonDamageEffect] No targets found! Returning null.");
        return null;
    }
}
