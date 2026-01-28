using System.Collections.Generic;
using UnityEngine;

public class SpecificHeroTM : TargetMode
{
    [SerializeField] private string heroName;

    public override List<CombatantView> GetTargets()
    {
        List<CombatantView> targets = new();
        var aliveHeroes = HeroSystem.Instance.GetAliveHeroViews();
        
        foreach (var hero in aliveHeroes)
        {
            if (hero.HeroInstance != null)
            {
                // Debug.Log($"[SpecificHeroTM] Checking hero '{hero.HeroInstance.Data.name}' against '{heroName}'");
                if (hero.HeroInstance.Data.name.IndexOf(heroName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    targets.Add(hero);
                    Debug.Log($"[SpecificHeroTM] FOUND Match: {hero.name}");
                }
            }
        }

        if (targets.Count == 0)
        {
            Debug.LogWarning($"[SpecificHeroTM] No alive hero found matching '{heroName}'");
        }
        
        return targets;
    }
}
