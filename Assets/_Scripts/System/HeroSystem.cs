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
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction,ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction,ReactionTiming.POST);
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDealDamagePostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction,ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction,ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDealDamagePostReaction, ReactionTiming.POST);
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

        Debug.Log($"[HeroSystem] BATTLE DATA: Have {heroesData.Count} heroes to place. Found {HeroViews.Count} slots (HeroView scripts) in the scene.");

        if (HeroViews.Count == 0)
        {
            Debug.LogError("[HeroSystem] CRITICAL: No HeroView objects found! Heroes will not be visible.");
            return;
        }

        if (heroesData.Count > HeroViews.Count)
        {
            Debug.LogWarning($"[HeroSystem] WARNING: You have {heroesData.Count} active heroes but only {HeroViews.Count} slots (HeroView objects) in the scene! Only the first {HeroViews.Count} will be shown.");
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

    //reacciones 
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        foreach (var heroView in HeroViews)
        {
            if (!heroView.gameObject.activeSelf) continue;

            int burnStacks = heroView.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
            {
                ApplyBurnGA applyBurnGA = new(burnStacks, heroView);
                ActionSystem.Instance.AddReaction(applyBurnGA);
            }
        }
        
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}
