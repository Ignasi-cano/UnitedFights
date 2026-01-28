using System;
using UnityEngine;

[System.Serializable]
public abstract class PerkCondition
{
    [field: SerializeField] public ReactionTiming ReactionTiming { get; protected set; }
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMet(GameAction gameAction);
     
}
