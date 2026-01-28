using UnityEngine;
using System;

[Serializable]
public abstract class MapEventOutcome
{
    public abstract void Execute();
    public abstract string GetResultText();
}

[Serializable]
public class GiveGoldOutcome : MapEventOutcome
{
    [SerializeField] private int amount;

    public override void Execute()
    {
        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.AddGold(amount);
        }
    }

    public override string GetResultText() => amount >= 0 ? $"Gain {amount} Gold" : $"Lose {Mathf.Abs(amount)} Gold";
}

[Serializable]
public class HealOutcome : MapEventOutcome
{
    [SerializeField] private int amount;

    public override void Execute()
    {
        if (HeroSystem.Instance != null && HeroSystem.Instance.MainHeroView != null)
        {
            if (amount >= 0) 
            {
                HeroSystem.Instance.MainHeroView.Heal(amount);
            }
            else 
            {
                HeroSystem.Instance.MainHeroView.Damage(Mathf.Abs(amount));
            }
        }
    }

    public override string GetResultText() => amount >= 0 ? $"Heal {amount} HP" : $"Lose {Mathf.Abs(amount)} HP";
}

[Serializable]
public class AddPerkOutcome : MapEventOutcome
{
    [SerializeField] private PerkData perkData;

    public override void Execute()
    {
        // 1. Add to Persistent Master List (Critical for next run)
        if (GameManager.Instance != null && perkData != null)
        {
            GameManager.Instance.AddPerkToMasterPerks(perkData);
        }
        else
        {
            Debug.LogError("[AddPerkOutcome] GameManager not found! Perk update won't persist.");
        }

        // 2. Add to Local System (Visible immediately if UI reads from this)
        if (PerkSystem.Instance != null && perkData != null)
        {
            PerkSystem.Instance.AddPerk(new Perk(perkData));
        }
    }

    public override string GetResultText() => $"Gain Perk: {(perkData != null ? (string.IsNullOrEmpty(perkData.Name) ? perkData.name : perkData.Name) : "None")}";
}

[Serializable]
public class AddCardOutcome : MapEventOutcome
{
    [SerializeField] private CardData cardData;

    public override void Execute()
    {
        // 1. Add to Persistent Master List (Critical for next run)
        if (GameManager.Instance != null && cardData != null)
        {
            GameManager.Instance.AddCardToMasterDeck(cardData);
        }
        else
        {
             Debug.LogError("[AddCardOutcome] GameManager not found! Card update won't persist.");
        }

        // 2. Add to Local System
        if (CardSystem.Instance != null && cardData != null)
        {
            CardSystem.Instance.AddCardToDeck(cardData);
        }
    }

    public override string GetResultText() => $"Gain Card: {(cardData != null ? cardData.name : "None")}";
}

[Serializable]
public class ChangeHandSizeOutcome : MapEventOutcome
{
    [SerializeField] private int changeAmount;

    public override void Execute()
    {
        if (HeroSystem.Instance != null)
        {
            HeroSystem.Instance.MaxHandSize += changeAmount;
            Debug.Log($"[HandSizeOutcome] Hand size changed by {changeAmount}. New size: {HeroSystem.Instance.MaxHandSize}");
        }
    }

    public override string GetResultText() => changeAmount >= 0 ? $"Increase Max Hand by {changeAmount}" : $"Decrease Max Hand by {Mathf.Abs(changeAmount)}";
}
