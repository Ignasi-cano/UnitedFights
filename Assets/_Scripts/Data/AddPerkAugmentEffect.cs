using System;
using UnityEngine;

[Serializable]
public class AddPerkAugmentEffect : AugmentEffect
{
    public PerkData Perk;

    public override void Execute()
    {
        if (Perk == null) return;
        
        GameManager.Instance.AddPerkToMasterPerks(Perk);
        
        // If it's an instant effect, apply it
        if (Perk.PerkCondition is InstantPerkCondition)
        {
            Perk.AutoTargetEffect.Effect.ApplyToInstances(GameManager.Instance.ActiveHeroes);
        }

        if (PerkSystem.HasInstance)
        {
            PerkSystem.Instance.AddPerk(new Perk(Perk));
        }
        
        Debug.Log($"[AddPerkAugmentEffect] Applied Perk: {Perk.name}");
    }
}
