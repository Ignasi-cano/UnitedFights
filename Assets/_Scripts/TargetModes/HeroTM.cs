using System.Collections.Generic;
using UnityEngine;

public class HeroTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        List<CombatantView> targets = new();
        foreach (var hv in HeroSystem.Instance.HeroViews)
        {
            if (hv.gameObject.activeSelf && hv.CurrentHealth > 0) targets.Add(hv);
        }
        return targets;
    }
}
