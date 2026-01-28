using UnityEngine;
using System.Collections.Generic;

public class HeroSystem : Singleton<HeroSystem>
{
    [field: SerializeField] public List<HeroView> HeroViews { get; private set; }
    public HeroView MainHeroView => HeroViews.Find(hv => hv.gameObject.activeSelf);

    public bool IsAnyHeroAlive => HeroViews.Exists(hv => hv.gameObject.activeSelf && hv.CurrentHealth > 0);

    public List<HeroView> GetAliveHeroViews()
    {
        return HeroViews.FindAll(hv => hv.gameObject.activeSelf && hv.CurrentHealth > 0);
    }

    public HeroView GetRandomHeroView()
    {
        var aliveHeroes = GetAliveHeroViews();
        if (aliveHeroes.Count == 0) return null;
        return aliveHeroes[Random.Range(0, aliveHeroes.Count)];
    }
    void OnEnable()
    {
        ActionSystem.SubscribeReaction<HeroTurnStartGA>(OnHeroTurnStartReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDealDamagePostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<HeroTurnStartGA>(OnHeroTurnStartReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDealDamagePostReaction, ReactionTiming.POST);
    }

    [field: SerializeField] public int MaxHandSize { get; set; } = 5;
    public bool HasBlackCandle { get; set; }

    private void OnHeroTurnStartReaction(HeroTurnStartGA action)
    {
        // 1. Discard old hand
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);

        // 2. Clear Armor
        ClearArmorGA clearArmorGA = new(new List<CombatantView>(GetAliveHeroViews()));
        ActionSystem.Instance.AddReaction(clearArmorGA);

        foreach (var heroView in HeroViews)
        {
            if (!heroView.gameObject.activeSelf) continue;

            int burnStacks = heroView.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
            {
                ApplyBurnGA applyBurnGA = new(burnStacks, heroView);
                ActionSystem.Instance.AddReaction(applyBurnGA);
            }

            int poisonStacks = heroView.GetStatusEffectStacks(StatusEffectType.POISON);
            if (poisonStacks > 0)
            {
                ApplyPoisonGA applyPoisonGA = new(heroView);
                ActionSystem.Instance.AddReaction(applyPoisonGA);
            }
        }
        
        int finalHandSize = MaxHandSize;
        if (CardSystem.Instance != null)
        {
            finalHandSize += CardSystem.Instance.GetTotalHandSizeModifier();
        }
        
        DrawCardsGA drawCardsGA = new(Mathf.Max(1, finalHandSize));
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }

    private void OnDealDamagePostReaction(DealDamageGA dealDamageGA)
    {
        if (!IsAnyHeroAlive)
        {
            Debug.Log("[HeroSystem] All heroes are dead! Game Over.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
        }
    }
    
    public void Setup(List<HeroInstance> heroesData)
    {
        Debug.Log($"[HeroSystem] Setup called with {heroesData.Count} HeroData objects.");
        
        if (HeroViews == null || HeroViews.Count == 0)
        {
            Debug.LogWarning("[HeroSystem] HeroViews list was EMPTY. Attempting to auto-find HeroView components in scene...");
            HeroViews = new List<HeroView>(FindObjectsByType<HeroView>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
        }

        if (HeroViews == null || HeroViews.Count == 0)
        {
            Debug.LogError("[HeroSystem] No HeroView components found in scene! Please create a Hero object with the HeroView script.");
            return;
        }

        for (int i = 0; i < HeroViews.Count; i++)
        {
            if (i < heroesData.Count)
            {
                HeroViews[i].gameObject.SetActive(true);
                HeroViews[i].Setup(heroesData[i]);
            }
            else
            {
                HeroViews[i].gameObject.SetActive(false);
            }
        }
    }

    public void SaveHeroesHealth()
    {
        foreach (var hv in HeroViews)
        {
            if (hv.gameObject.activeSelf && hv.HeroInstance != null)
            {
                hv.HeroInstance.CurrentHealth = hv.CurrentHealth;
                Debug.Log($"[HeroSystem] Saved {hv.HeroInstance.Data.name} health: {hv.CurrentHealth}");
            }
        }
    }
}
