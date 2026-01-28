using System;
using UnityEngine;

[Serializable]
public class AddGoldAugmentEffect : AugmentEffect
{
    public int GoldAmount;

    public override void Execute()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.AddGold(GoldAmount);
            Debug.Log($"[AddGoldAugmentEffect] Added {GoldAmount} gold.");
        }
    }
}
