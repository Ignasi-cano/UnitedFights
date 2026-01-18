using System.Collections.Generic;
using UnityEngine;
using SerializeReferenceEditor;

[System.Serializable]
public class MultiEffect : Effect
{
    [SerializeReference, SR] public List<Effect> effects = new();

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        List<GameAction> actions = new();
        foreach (var effect in effects)
        {
            if (effect != null)
            {
                actions.Add(effect.GetGameAction(targets, caster));
            }
        }
        return new MultiActionGA(actions);
    }
}
