using System;

[System.Serializable]
public class InstantPerkCondition : PerkCondition
{
    public override bool SubConditionIsMet(GameAction gameAction)
    {
        // For instant perks, we trigger when gameAction is null (called manually on subscribe)
        return gameAction == null;
    }

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        // Trigger the reaction immediately with a null action or a dummy action
        // Perk.Reaction checks if subcondition is met, so we need to handle that.
        // Actually, let's make it smarter:
        reaction?.Invoke(null);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        // Nothing to unsubscribe
    }
}
