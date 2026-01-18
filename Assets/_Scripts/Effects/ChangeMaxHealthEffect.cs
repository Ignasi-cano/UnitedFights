using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChangeMaxHealthEffect : Effect
{
    [SerializeField] private int amount;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new ChangeMaxHealthGA(targets, amount);
    }

    public override void ApplyToInstances(List<HeroInstance> targets)
    {
        foreach (var hero in targets)
        {
            hero.MaxHealthBonus += amount;
            hero.CurrentHealth += amount; // User requested: 10/40 + 15 -> 25/55
            Debug.Log($"[ChangeMaxHealthEffect] Applied to instance {hero.Data.name}. MaxBonus: {hero.MaxHealthBonus}, Current: {hero.CurrentHealth}");
        }
    }
}
