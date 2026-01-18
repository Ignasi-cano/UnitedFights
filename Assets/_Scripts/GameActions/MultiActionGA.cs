using System.Collections.Generic;

public class MultiActionGA : GameAction
{
    public MultiActionGA(List<GameAction> actions)
    {
        if (actions == null) return;
        
        foreach (var action in actions)
        {
            if (action != null) PerformReactions.Add(action);
        }
    }
}
