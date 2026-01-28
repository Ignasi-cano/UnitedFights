using System.Collections.Generic;
using UnityEngine;

public class Perk
{
    public Sprite Image => data.Image;
    public string Name => string.IsNullOrEmpty(data.Name) ? data.name : data.Name;
    public string Description => data.Description;
    public PerkData Data => data;
    private readonly PerkData data;
    private readonly PerkCondition condition;
    private readonly AutoTargetEffect effect;
    public Perk(PerkData perkData)
    {
        data = perkData;
        condition = data.PerkCondition;
        effect = data.AutoTargetEffect;
    }
    public void OnAdd()
    {
        Debug.Log($"[Perk] OnAdd called for {data.name}");
        
        // Execute initialization effects
        if (data.OnAddEffects != null)
        {
            foreach (var effect in data.OnAddEffects)
            {
                effect.GetGameAction(new List<CombatantView>(), HeroSystem.Instance.MainHeroView);
            }
        }

        if (condition != null)
        {
            condition.SubscribeCondition(Reaction);
        }
        else
        {
            Debug.LogWarning($"[Perk] {data.name} has no PerkCondition assigned! It will be passive but won't react to game actions.");
        }
    }
    public void OnRemove()
    {
        if (condition != null)
        {
            condition.UnsubscribeCondition(Reaction);
        }
    }
    private void Reaction(GameAction gameAction)
    {
        if(condition.SubConditionIsMet(gameAction))
        {
            if (effect == null || effect.Effect == null) return;

            List<CombatantView> targets = new();
            if (data.UseActionCasterAsTarget && gameAction is IHaveCaster haveCaster)
            {
                targets.Add(haveCaster.Caster);
            }
            if (data.UseActionTargets && gameAction is IHaveTargets haveTargets)
            {
                foreach (var t in haveTargets.Targets)
                {
                    if (!targets.Contains(t)) targets.Add(t);
                }
            }
            if(data.UseAutoTarget)
            {
                if (effect != null && effect.TargetMode != null)
                {
                    targets.AddRange(effect.TargetMode.GetTargets());
                }
            }
            
            GameAction perkEffectAction = effect.Effect.GetGameAction(targets, HeroSystem.Instance.MainHeroView);
            
            if (perkEffectAction != null)
            {
                ActionSystem.Instance.AddReaction(perkEffectAction);
            }
        }
    }
}
