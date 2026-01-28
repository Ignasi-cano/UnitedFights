using System;
using UnityEngine;

[Serializable]
public class OnCursedCardDrawnCondition : PerkCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<CardDrawnGA>(reaction, ReactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<CardDrawnGA>(reaction, ReactionTiming);
    }

    public override bool SubConditionIsMet(GameAction gameAction)
    {
        if (gameAction is CardDrawnGA drawAction)
        {
            string cardName = drawAction.Card != null ? (drawAction.Card.Data != null ? drawAction.Card.Data.name : "Unknown Data") : "Null Card";
            bool isCursed = drawAction.Card != null && drawAction.Card.Data != null && drawAction.Card.Data.IsCursed;
            
            // Log EVERYTHING to be sure
            Debug.Log($"[BlackCandle_Debug] Checking drawn card: '{cardName}'. IsCursed: {isCursed}");

            if (isCursed) Debug.Log($"[OnCursedCardDrawnCondition] Cursed card detected: {cardName}");
            return isCursed;
        }
        return false;
    }
}
