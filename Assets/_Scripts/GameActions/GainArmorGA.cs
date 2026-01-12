using System.Collections.Generic;
using UnityEngine;

public class GainArmorGA : GameAction
{
    public int Amount { get; set; }
    public List<CombatantView> Target { get; set; }
    public GainArmorGA(int amount, List<CombatantView> target)
    {
        Amount = amount;
        Target=new(target);
    }
}
