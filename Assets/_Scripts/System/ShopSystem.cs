using UnityEngine;
using System.Collections.Generic;

public class ShopSystem : Singleton<ShopSystem>
{
    public bool BuyHero(HeroData hero)
    {
        if (CurrencySystem.Instance.TrySpendGold(hero.Cost))
        {
            if (GameManager.Instance.TryAddHero(hero, false))
            {
                Debug.Log($"Shop: Successfully bought hero {hero.name}");
                
                // NEW: Sync hero to Firebase Inventory
                if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
                {
                    if (ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddToInventory(AuthManager.Instance.CurrentUser.UserId, "Hero", hero.name);
                    }
                    else
                    {
                        Debug.LogWarning("[ShopSystem] ScoreManager instance missing, could not sync hero to cloud.");
                    }
                }
                
                return true;
            }
        }
        return false;
    }

    public bool BuyCard(CardData card)
    {
        if (CurrencySystem.Instance.TrySpendGold(card.Cost))
        {
            GameManager.Instance.AddCardToMasterDeck(card);
            Debug.Log($"Shop: Successfully bought card {card.name}");
            return true;
        }
        return false;
    }

    public bool BuyPerk(PerkData perkData)
    {
        if (GameManager.Instance.MasterPerks.Count >= GameManager.MAX_PERKS)
        {
            Debug.LogWarning("[ShopSystem] Cannot buy perk: Maximum perk limit reached (10).");
            return false;
        }

        if (CurrencySystem.Instance.TrySpendGold(perkData.Cost))
        {
            // 1. Persist it globally
            GameManager.Instance.MasterPerks.Add(perkData);

            // 2. If it's an "Instant" perk (like HP), apply it immediately to HeroInstance data
            if (perkData.PerkCondition is InstantPerkCondition)
            {
                if (perkData.AutoTargetEffect != null && perkData.AutoTargetEffect.Effect != null)
                {
                    perkData.AutoTargetEffect.Effect.ApplyToInstances(GameManager.Instance.ActiveHeroes);
                }
                else
                {
                    Debug.LogWarning($"[ShopSystem] Perk {perkData.Name} is Instant but has no AutoTargetEffect/Effect assigned.");
                }
            }

            // 3. Add to live system if we are in a scene that has one (optional/safety)
            if (PerkSystem.Instance != null)
            {
                PerkSystem.Instance.AddPerk(new Perk(perkData));
            }

            Debug.Log($"Shop: Successfully bought perk {perkData.name} and applied persistent effects.");
            return true;
        }
        return false;
    }

    public bool BuyCardRemoval(int cost, System.Action onSuccess = null)
    {
        if (CurrencySystem.Instance.Gold < cost)
        {
            Debug.LogWarning("[ShopSystem] Not enough gold for card removal.");
            return false;
        }

        if (CardSelectionUI.Instance != null)
        {
            List<CardData> deck = GameManager.Instance.MasterDeck;
            CardSelectionUI.Instance.Open("Select Card to Remove", deck, (cardToRemove) => {
                if (CurrencySystem.Instance.TrySpendGold(cost))
                {
                    GameManager.Instance.RemoveCardFromMasterDeck(cardToRemove);
                    onSuccess?.Invoke();
                }
            });
            return false; // Return false so the shop doesn't mark it as sold immediately
        }
        else
        {
            Debug.LogError("[ShopSystem] CardSelectionUI instance not found in scene!");
            return false;
        }
    }
}
