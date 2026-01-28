using System.Collections.Generic;
using UnityEngine;

public class RandomAllyTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        List<CombatantView> targets = new();
        var aliveHeroes = HeroSystem.Instance.GetAliveHeroViews();
        Debug.Log($"[RandomAllyTM] Found {aliveHeroes.Count} alive heroes.");
        
        if (aliveHeroes.Count > 0)
        {
            var target = aliveHeroes[Random.Range(0, aliveHeroes.Count)];
            targets.Add(target);
            Debug.Log($"[RandomAllyTM] Selected target: {target.name}");
        }
        else
        {
            Debug.LogWarning("[RandomAllyTM] No alive heroes found!");
        }
        return targets;
    }
}
