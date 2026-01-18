using System.Collections.Generic;
using UnityEngine;

public class GiveGoldEffect : Effect
{
    [SerializeField] private int amount;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        Debug.Log($"[GiveGoldEffect] Creating GiveGoldGA with amount: {amount}");
        return new GiveGoldGA(amount);
    }
}
