using System;
using UnityEngine;

public class OnActionCondition : PerkCondition
{
    [SerializeField] private string actionTypeName;

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        Type type = Type.GetType(actionTypeName);
        if (type == null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(actionTypeName);
                if (type != null) break;
            }
        }

        if (type == null)
        {
            Debug.LogError($"[OnActionCondition] Action type '{actionTypeName}' not found! Make sure the name is correct.");
            return;
        }
        ActionSystem.SubscribeReaction(type, reaction, ReactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        Type type = Type.GetType(actionTypeName);
        if (type == null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(actionTypeName);
                if (type != null) break;
            }
        }

        if (type != null)
        {
            ActionSystem.UnsubscribeReaction(type, reaction, ReactionTiming);
        }
    }

    public override bool SubConditionIsMet(GameAction gameAction)
    {
        string currentActionName = gameAction.GetType().Name;
        // Trim both to avoid silly whitespace errors
        string targetName = actionTypeName.Trim();
        bool isMatch = currentActionName == targetName;
        
        Debug.Log($"[OnActionCondition] Checking: '{currentActionName}' == '{targetName}' ? {isMatch}");
        return isMatch;
    }
}
