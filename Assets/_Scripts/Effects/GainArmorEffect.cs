using System.Collections.Generic;
using UnityEngine;

public class GainArmorEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<CombatantView> target, CombatantView caster)
    {
        GainArmorGA gainArmorGA = new(amount, target);
        return gainArmorGA;
    }
}
