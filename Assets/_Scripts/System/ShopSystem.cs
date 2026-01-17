using UnityEngine;
using System.Collections.Generic;

public class ShopSystem : Singleton<ShopSystem>
{
    public bool BuyHero(HeroData hero)
    {
        if (GameManager.Instance.ActiveHeroes.Count >= GameManager.MAX_HEROES)
        {
            Debug.LogWarning("Shop: Already at max heroes!");
            return false;
        }

        if (CurrencySystem.Instance.TrySpendGold(hero.Cost))
        {
            if (GameManager.Instance.TryAddHero(hero))
            {
                Debug.Log($"Shop: Successfully bought hero {hero.name}");
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
        if (CurrencySystem.Instance.TrySpendGold(perkData.Cost))
        {
            PerkSystem.Instance.AddPerk(new Perk(perkData));
            Debug.Log($"Shop: Successfully bought perk {perkData.name}");
            return true;
        }
        return false;
    }

    public bool BuyCardRemoval(int cost)
    {
        if (CurrencySystem.Instance.TrySpendGold(cost))
        {
            if (CardSelectionUI.Instance != null)
            {
                List<CardData> deck = GameManager.Instance.MasterDeck;
                CardSelectionUI.Instance.Open("Select Card to Remove", deck, (cardToRemove) => {
                    GameManager.Instance.RemoveCardFromMasterDeck(cardToRemove);
                });
                return true;
            }
        }
        return false;
    }
}
