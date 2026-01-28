using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    [SerializeField] private List<HeroData> availableHeroes = new List<HeroData>();
    public List<HeroData> AvailableHeroes => availableHeroes;
    [SerializeField] private List<HeroInstance> activeHeroes = new List<HeroInstance>();
    public List<HeroInstance> ActiveHeroes => activeHeroes;
    public const int MAX_HEROES = 3;

    // Master list of cards that persists between scenes
    private List<CardData> masterDeck = new List<CardData>();
    public List<CardData> MasterDeck => masterDeck;

    private List<PerkData> masterPerks = new List<PerkData>();
    public List<PerkData> MasterPerks => masterPerks;
    public const int MAX_PERKS = 10;

    private List<AugmentEffect> activeAugments = new();

    protected override void Awake()
    {
        base.Awake();
        
        // If we were destroyed by the Singleton pattern, don't continue
        if (Instance != this) return;

        MapSystem.OnNodeSelected += HandleNodeSelected;

        Debug.Log($"[GameManager] Awake on {gameObject.name}. Hero count: {(availableHeroes != null ? availableHeroes.Count : 0)}");

        // Ensure we have at least one hero if none selected (for testing)
        if (activeHeroes.Count == 0 && availableHeroes != null && availableHeroes.Count > 0)
        {
            HeroData defaultHeroData = availableHeroes[0];
            HeroInstance defaultHero = new HeroInstance(defaultHeroData);
            activeHeroes.Add(defaultHero);
            if (defaultHeroData.Deck != null) masterDeck.AddRange(defaultHeroData.Deck);
            if (defaultHeroData.StartingPerks != null) masterPerks.AddRange(defaultHeroData.StartingPerks);
            Debug.Log($"[GameManager] Added default hero: {defaultHeroData.name} and their cards/perks to Master Deck.");
        }
    }

    public bool TryAddHero(HeroData heroData, bool includeStartingPerks = true)
    {
        // Check if we already have this hero type to avoid duplicate decks
        bool alreadyOwned = activeHeroes.Exists(h => h != null && h.Data.name == heroData.name);

        HeroInstance heroInstance = new HeroInstance(heroData);
        activeHeroes.Add(heroInstance);
        Debug.Log($"[GameManager] Hero added to collection: {heroData.name}. Total owned: {activeHeroes.Count}");
        
        // Add hero cards and perks to the master lists ONLY if it's the first one of its kind
        if (!alreadyOwned)
        {
            if (heroData.Deck != null)
            {
                masterDeck.AddRange(heroData.Deck);
                Debug.Log($"[GameManager] First time owning {heroData.name}. Cards added to Master Deck.");
            }
             if (includeStartingPerks && heroData.StartingPerks != null)
            {
                masterPerks.AddRange(heroData.StartingPerks);
                Debug.Log($"[GameManager] First time owning {heroData.name}. Perks added to Master Perks.");
            }
        }
        else
        {
            Debug.Log($"[GameManager] Already owned {heroData.name}. Cards/Perks NOT added to Master lists.");
            
            // NEW: Check for evolution if we now have 3 of these
            CheckEvolution(heroData);
        }
        
        return true;
    }

    public List<HeroInstance> GetUniqueActiveHeroes()
    {
        Dictionary<string, (HeroInstance instance, int tier)> uniqueLineages = new();

        foreach (var hero in activeHeroes)
        {
            if (hero == null || hero.Data == null) continue;

            string finalID = GetFinalEvolutionID(hero.Data);
            int tier = GetEvolutionTier(hero.Data);

            if (!uniqueLineages.ContainsKey(finalID) || tier > uniqueLineages[finalID].tier)
            {
                uniqueLineages[finalID] = (hero, tier);
            }
        }

        List<HeroInstance> result = new();
        foreach (var val in uniqueLineages.Values)
        {
            result.Add(val.instance);
        }
        return result;
    }

    private string GetFinalEvolutionID(HeroData data)
    {
        HeroData current = data;
        while (current.NextEvolution != null)
        {
            current = current.NextEvolution;
        }
        return current.name;
    }

    private int GetEvolutionTier(HeroData data)
    {
        // Simple tier: how many evolutions AFTER this one? 
        // We want the HIGHEST tier to have 0 steps remaining.
        // Actually, let's count steps from this to final. 
        // The more steps, the LOWER the tier.
        int steps = 0;
        HeroData current = data;
        while (current.NextEvolution != null)
        {
            current = current.NextEvolution;
            steps++;
        }
        // Tier 0 = Final Form, Tier 1 = Middle, Tier 2 = Base.
        // We want to pick the SMALLEST 'steps'.
        return -steps; // -0 is highest, -1 is lower, -2 is base.
    }

    private void CheckEvolution(HeroData baseData)
    {
        if (baseData.NextEvolution == null) return;

        // 1. Count instances of this exact hero data
        List<HeroInstance> matches = activeHeroes.FindAll(h => h.Data == baseData);
        
        if (matches.Count >= 3)
        {
            Debug.Log($"[GameManager] {baseData.name} Evolution Triggered! (3/3 owned)");

            // 2. Remove 3 instances of the base hero
            for (int i = 0; i < 3; i++)
            {
                activeHeroes.Remove(matches[i]);
            }

            // 3. Remove 1 set of cards/perks of the base hero 
            // (Since only the first one added cards, and we are replacing it with a better version)
            int removedCards = 0;
            if (baseData.Deck != null)
            {
                foreach (var card in baseData.Deck)
                {
                    if (masterDeck.Remove(card)) removedCards++;
                }
            }

            int removedPerks = 0;
            if (baseData.StartingPerks != null)
            {
                foreach (var perk in baseData.StartingPerks)
                {
                    if (masterPerks.Remove(perk)) removedPerks++;
                }
            }
            
            Debug.Log($"[GameManager] Deck cleanup for {baseData.name} complete: Removed {removedCards} cards and {removedPerks} perks.");

            // 4. Add the evolved hero
            Debug.Log($"[GameManager] Evolving {baseData.name} -> {baseData.NextEvolution.name}");
            TryAddHero(baseData.NextEvolution);
        }
    }

    public void AddCardToMasterDeck(CardData card)
    {
        masterDeck.Add(card);
        Debug.Log($"[GameManager] Card added to Master Deck: {card.name}");
    }

    public void AddPerkToMasterPerks(PerkData perk)
    {
        masterPerks.Add(perk);
        Debug.Log($"[GameManager] Perk added to Master Perks: {perk.name}");
    }

    public void RemoveCardFromMasterDeck(CardData card)
    {
        masterDeck.Remove(card);
        Debug.Log($"[GameManager] Card removed from Master Deck: {card.name}");
    }

    public void SelectHero(HeroData heroData)
    {
        activeHeroes.Clear();
        masterDeck.Clear();
        masterPerks.Clear();
        
        HeroInstance heroInstance = new HeroInstance(heroData);
        activeHeroes.Add(heroInstance);
        if (heroData.Deck != null) masterDeck.AddRange(heroData.Deck);
        if (heroData.StartingPerks != null) masterPerks.AddRange(heroData.StartingPerks);
        
        Debug.Log($"Hero selected and set as active: {heroData.name}. Initial cards and perks added to Master lists.");
    }

    public void AddAugment(AugmentEffect effect)
    {
        activeAugments.Add(effect);
        effect.Execute(); // Perform initial execution
        Debug.Log($"[GameManager] Augment added: {effect.GetType().Name}");
    }

    private void HandleNodeSelected(MapNode node)
    {
        foreach (var augment in activeAugments)
        {
            augment.OnNodeEntry(node);
        }
    }
}
