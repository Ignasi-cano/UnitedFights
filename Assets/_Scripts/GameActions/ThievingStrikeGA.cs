using System.Collections.Generic;
using UnityEngine;

public class ThievingStrikeGA : GameAction
{
    public int Damage { get; private set; }
    public int Gold { get; private set; }
    public List<CombatantView> Targets { get; private set; }

    public ThievingStrikeGA(int damage, int gold, List<CombatantView> targets)
    {
        Damage = damage;
        Gold = gold;
        Targets = targets;
    }
}
