using System;
using UnityEngine;

[Serializable]
public class UpgradePerkAugmentEffect : AugmentEffect
{
    public PerkData OriginalPerk;
    public PerkData UpgradedPerk;

    public override void Execute()
    {
        if (OriginalPerk == null || UpgradedPerk == null)
        {
            Debug.LogWarning("[UpgradePerkAugmentEffect] OriginalPerk or UpgradedPerk is null. Cannot upgrade.");
            return;
        }

        // 1. Swap in Master Perks (for persistence across runs/nodes)
        if (GameManager.Instance.MasterPerks.Contains(OriginalPerk))
        {
            GameManager.Instance.MasterPerks.Remove(OriginalPerk);
            // Verify we don't duplicate if for some reason it's already there (unlikely but safe)
            if (!GameManager.Instance.MasterPerks.Contains(UpgradedPerk))
            {
                GameManager.Instance.MasterPerks.Add(UpgradedPerk);
            }
            Debug.Log($"[UpgradePerkAugmentEffect] Upgraded {OriginalPerk.name} to {UpgradedPerk.name} in MasterPerks");
        }

        // 2. Swap in Active Perks (if currently active in the perk system)
        if (PerkSystem.HasInstance)
        {
            Perk activePerk = PerkSystem.Instance.GetPerk(OriginalPerk);
            if (activePerk != null)
            {
                PerkSystem.Instance.RemovePerk(activePerk);
                PerkSystem.Instance.AddPerk(new Perk(UpgradedPerk));
                Debug.Log($"[UpgradePerkAugmentEffect] Upgraded {OriginalPerk.name} to {UpgradedPerk.name} in Active Perks");
            }
        }
    }
}
