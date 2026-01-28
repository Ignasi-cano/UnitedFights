using System;
using UnityEngine;

[System.Serializable]
public class StatusEffectStackCondition : PerkCondition
{
    [field: SerializeField] public StatusEffectType Type { get; private set; }
    [field: SerializeField] public int Threshold { get; private set; }

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction(typeof(AddStatusEffectGa), reaction, ReactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction(typeof(AddStatusEffectGa), reaction, ReactionTiming);
    }

    public override bool SubConditionIsMet(GameAction gameAction)
    {
        if (gameAction is AddStatusEffectGa addStatusEffectGa)
        {
            if (addStatusEffectGa.StatusEffectType != Type) return false;

            foreach (var target in addStatusEffectGa.Targets)
            {
                if (target.GetStatusEffectStacks(Type) >= Threshold)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
