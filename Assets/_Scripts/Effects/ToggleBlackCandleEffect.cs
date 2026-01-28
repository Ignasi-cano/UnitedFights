using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ToggleBlackCandleEffect : Effect
{
    [SerializeField] private bool activeState = true;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (HeroSystem.Instance != null)
        {
            HeroSystem.Instance.HasBlackCandle = activeState;
            Debug.Log($"[Black Candle] Penalty flag set to: {activeState}");
        }
        return null; // This is a utility effect, no action needed
    }
}
