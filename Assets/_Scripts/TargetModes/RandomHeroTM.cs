using System.Collections.Generic;
using UnityEngine;

public class RandomHeroTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        HeroView target = HeroSystem.Instance.GetRandomHeroView();
        return new() { target };
    }
}
