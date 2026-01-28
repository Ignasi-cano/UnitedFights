using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class HealOnNodeEntryAugmentEffect : AugmentEffect
{
    public int HealAmount = 5;

    public override void Execute()
    {
        Debug.Log("[HealOnNodeEntryAugmentEffect] Activated. Will heal random ally on node entry.");
    }

    public override void OnNodeEntry(MapNode node)
    {
        if (GameManager.Instance == null) return;

        List<HeroInstance> heroes = GameManager.Instance.ActiveHeroes;
        if (heroes == null || heroes.Count == 0) return;

        // 1. Filter heroes with missing health
        List<HeroInstance> woundedHeroes = heroes.FindAll(h => h.CurrentHealth < h.GetMaxHealth());

        HeroInstance target = null;

        if (woundedHeroes.Count > 0)
        {
            // Pick a random wounded hero
            int randomIndex = UnityEngine.Random.Range(0, woundedHeroes.Count);
            target = woundedHeroes[randomIndex];
        }
        else
        {
            // If no one is wounded, don't waste the heal effect
            Debug.Log("[HealOnNodeEntryAugmentEffect] No wounded heroes found. Skipping heal.");
            return;
        }

        // 2. Perform Heal
        int oldHP = target.CurrentHealth;
        target.CurrentHealth = Mathf.Min(target.CurrentHealth + HealAmount, target.GetMaxHealth());
        
        Debug.Log($"[HealOnNodeEntryAugmentEffect] Node {node.ID} entered. Targeted wounded ally {target.Data.name}. Healed for {HealAmount} HP ({oldHP} -> {target.CurrentHealth})");
    }
}
