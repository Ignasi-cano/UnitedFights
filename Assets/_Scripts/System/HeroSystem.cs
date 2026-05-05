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

    public HeroView GetRandomFrontlineHero()
    {
        List<HeroView> frontline = new();

        for (int i = 0; i < Mathf.Min(2, HeroViews.Count); i++)
        {
            if (IsAlive(i))
                frontline.Add(HeroViews[i]);
        }

        if (frontline.Count > 0)
            return frontline[Random.Range(0, frontline.Count)];

        List<HeroView> backline = new();

        for (int i = 2; i < HeroViews.Count; i++)
        {
            if (IsAlive(i))
                backline.Add(HeroViews[i]);
        }

        if (backline.Count > 0)
            return backline[Random.Range(0, backline.Count)];

        return null;
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
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);

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
        Debug.Log($"[HeroSystem] Setup called with {heroesData.Count} slot entries.");

        if (HeroViews == null || HeroViews.Count == 0)
        {
            Debug.LogWarning("[HeroSystem] HeroViews list was EMPTY. Attempting to auto-find HeroView components in scene...");
            HeroViews = new List<HeroView>(FindObjectsByType<HeroView>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));

            // Keep scene hierarchy order stable
            HeroViews.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        }

        if (HeroViews == null || HeroViews.Count == 0)
        {
            Debug.LogError("[HeroSystem] No HeroView components found in scene! Please create Hero objects with the HeroView script.");
            return;
        }

        for (int i = 0; i < HeroViews.Count; i++)
        {
            bool hasValidHero =
                i < heroesData.Count &&
                heroesData[i] != null &&
                heroesData[i].Data != null;

            if (hasValidHero)
            {
                heroesData[i].Position = (i < 2) ? SlotPosition.Frontline : SlotPosition.Backline;

                HeroViews[i].gameObject.SetActive(true);
                HeroViews[i].Setup(heroesData[i]);

                Debug.Log($"[HeroSystem] Slot {i} active with {heroesData[i].Data.name} ({heroesData[i].Position})");
            }
            else
            {
                HeroViews[i].gameObject.SetActive(false);
                Debug.Log($"[HeroSystem] Slot {i} is EMPTY -> disabling {HeroViews[i].name}");
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

    public void HandleFrontlineDeath(HeroView deadHero)
    {
        int index = HeroViews.IndexOf(deadHero);

        // Only react if it was a frontline slot
        if (index < 0 || index >= 2) return;

        int preferredBackIndex = index + 2;

        // 1. Prefer the matching backline hero behind it
        if (IsAlive(preferredBackIndex))
        {
            SwapHeroes(index, preferredBackIndex);
            return;
        }

        // 2. Otherwise find any backline hero alive
        for (int i = 2; i < HeroViews.Count; i++)
        {
            if (IsAlive(i))
            {
                SwapHeroes(index, i);
                return;
            }
        }
    }

    private bool IsAlive(int index)
    {
        return HeroViews[index] != null &&
               HeroViews[index].CurrentHealth > 0 &&
               HeroViews[index].gameObject.activeSelf;
    }

    private void SwapHeroes(int a, int b)
    {
        Debug.Log($"[HeroSystem] Swapping {HeroViews[a].name} with {HeroViews[b].name}");

        Vector3 posA = HeroViews[a].transform.position;
        Vector3 posB = HeroViews[b].transform.position;

        var temp = HeroViews[a];
        HeroViews[a] = HeroViews[b];
        HeroViews[b] = temp;

        HeroViews[a].transform.position = posA;
        HeroViews[b].transform.position = posB;
    }
}