using System.Collections.Generic;
using UnityEngine;

public class AddManaEffect : Effect
{
    [SerializeField] private int manaAmount;

    public override GameAction GetGameAction(
        List<CombatantView> targets,
        CombatantView caster
    )
    {
        return new AddManaGA(manaAmount);
    }
}
