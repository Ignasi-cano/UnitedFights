using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    [SerializeField] private List<HeroData> availableHeroes = new List<HeroData>();
    public List<HeroData> AvailableHeroes => availableHeroes;

    [Header("Team Slots")]
    [SerializeField] private HeroInstance[] frontlineSlots = new HeroInstance[2];
    [SerializeField] private HeroInstance[] backlineSlots = new HeroInstance[2];

    public HeroInstance[] FrontlineSlots => frontlineSlots;
    public HeroInstance[] BacklineSlots => backlineSlots;

    // Compatibility property for old code that still expects a flat list
    public List<HeroInstance> ActiveHeroes => GetOccupiedHeroes();

    public const int MAX_HEROES = 4;
    public const int MAX_PERKS = 10;

    // Master list of cards that persists between scenes
    private List<CardData> masterDeck = new List<CardData>();
    public List<CardData> MasterDeck => masterDeck;

    private List<PerkData> masterPerks = new List<PerkData>();
    public List<PerkData> MasterPerks => masterPerks;

    private List<AugmentEffect> activeAugments = new();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this) return;

        EnsureSlotArrays();
        MapSystem.OnNodeSelected += HandleNodeSelected;

        Debug.Log($"[GameManager] Awake on {gameObject.name}. Hero count: {(availableHeroes != null ? availableHeroes.Count : 0)}");

        // Ensure we have at least one hero if none selected (for testing)
        if (GetOccupiedHeroes().Count == 0 && availableHeroes != null && availableHeroes.Count > 0)
        {
            HeroData defaultHeroData = availableHeroes[0];
            TryAddHero(defaultHeroData, true);
            Debug.Log($"[GameManager] Added default hero: {defaultHeroData.name} and their cards/perks to Master Deck.");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            MapSystem.OnNodeSelected -= HandleNodeSelected;
        }
    }

    private void EnsureSlotArrays()
    {
        if (frontlineSlots == null || frontlineSlots.Length != 2)
            frontlineSlots = new HeroInstance[2];

        if (backlineSlots == null || backlineSlots.Length != 2)
            backlineSlots = new HeroInstance[2];
    }

    private bool IsValidHeroInstance(HeroInstance hero)
    {
        return hero != null && hero.Data != null;
    }

    public bool TryAddHero(HeroData heroData, bool includeStartingPerks = true)
    {
        if (heroData == null)
        {
            Debug.LogWarning("[GameManager] TryAddHero failed: heroData is null.");
            return false;
        }

        EnsureSlotArrays();

        // DUPLICATE PURCHASE: increase evolution counter, do not consume a slot
        HeroInstance existingHero = FindHeroInstanceByData(heroData);
        if (existingHero != null)
        {
            existingHero.AddCopy();

            Debug.Log($"[GameManager] Duplicate purchased for {heroData.name}. Copies: {existingHero.EvolutionCopies}/3");

            TryEvolveHero(existingHero, includeStartingPerks);
            return true;
        }

        // NEW HERO: must fit in a slot
        if (GetOccupiedHeroes().Count >= MAX_HEROES)
        {
            Debug.LogWarning($"[GameManager] Cannot add {heroData.name}: team is full ({MAX_HEROES}/{MAX_HEROES}).");
            return false;
        }

        HeroInstance heroInstance = new HeroInstance(heroData);

        if (!TryPlaceHeroInFirstFreeSlot(heroInstance))
        {
            Debug.LogWarning($"[GameManager] Could not place hero {heroData.name} in any slot.");
            return false;
        }

        Debug.Log($"[GameManager] New hero added to team: {heroData.name}");

        if (heroData.Deck != null)
        {
            masterDeck.AddRange(heroData.Deck);
            Debug.Log($"[GameManager] Added {heroData.name} cards to Master Deck.");
        }

        if (includeStartingPerks && heroData.StartingPerks != null)
        {
            masterPerks.AddRange(heroData.StartingPerks);
            Debug.Log($"[GameManager] Added {heroData.name} perks to Master Perks.");
        }

        return true;
    }

    // Returns exactly 4 entries, preserving slot order:
    // 0 = Frontline Left
    // 1 = Frontline Right
    // 2 = Backline Left
    // 3 = Backline Right
    public List<HeroInstance> GetAllSlottedHeroes()
    {
        EnsureSlotArrays();

        return new List<HeroInstance>
        {
            IsValidHeroInstance(frontlineSlots[0]) ? frontlineSlots[0] : null,
            IsValidHeroInstance(frontlineSlots[1]) ? frontlineSlots[1] : null,
            IsValidHeroInstance(backlineSlots[0]) ? backlineSlots[0] : null,
            IsValidHeroInstance(backlineSlots[1]) ? backlineSlots[1] : null
        };
    }

    // Returns only occupied slots, preserving slot order.
    public List<HeroInstance> GetOccupiedHeroes()
    {
        EnsureSlotArrays();

        List<HeroInstance> result = new();

        if (IsValidHeroInstance(frontlineSlots[0])) result.Add(frontlineSlots[0]);
        if (IsValidHeroInstance(frontlineSlots[1])) result.Add(frontlineSlots[1]);
        if (IsValidHeroInstance(backlineSlots[0])) result.Add(backlineSlots[0]);
        if (IsValidHeroInstance(backlineSlots[1])) result.Add(backlineSlots[1]);

        return result;
    }

    // Kept for temporary compatibility with code that still calls it.
    public List<HeroInstance> GetUniqueActiveHeroes()
    {
        return GetOccupiedHeroes();
    }

    public void AddCardToMasterDeck(CardData card)
    {
        if (card == null) return;

        masterDeck.Add(card);
        Debug.Log($"[GameManager] Card added to Master Deck: {card.name}");
    }

    public void AddPerkToMasterPerks(PerkData perk)
    {
        if (perk == null) return;

        masterPerks.Add(perk);
        Debug.Log($"[GameManager] Perk added to Master Perks: {perk.name}");
    }

    public void RemoveCardFromMasterDeck(CardData card)
    {
        if (card == null) return;

        masterDeck.Remove(card);
        Debug.Log($"[GameManager] Card removed from Master Deck: {card.name}");
    }

    public void SelectHero(HeroData heroData)
    {
        EnsureSlotArrays();
        ClearAllHeroSlots();
        masterDeck.Clear();
        masterPerks.Clear();

        if (heroData == null)
        {
            Debug.LogWarning("[GameManager] SelectHero called with null HeroData.");
            return;
        }

        HeroInstance heroInstance = new HeroInstance(heroData);
        PlaceHeroInSpecificSlotIgnoringOccupancy(heroInstance, 0);

        if (heroData.Deck != null) masterDeck.AddRange(heroData.Deck);
        if (heroData.StartingPerks != null) masterPerks.AddRange(heroData.StartingPerks);

        Debug.Log($"[GameManager] Hero selected and set as active: {heroData.name}. Initial cards and perks added to Master lists.");
    }

    public void AddAugment(AugmentEffect effect)
    {
        if (effect == null) return;

        activeAugments.Add(effect);
        effect.Execute();
        Debug.Log($"[GameManager] Augment added: {effect.GetType().Name}");
    }

    private void HandleNodeSelected(MapNode node)
    {
        foreach (var augment in activeAugments)
        {
            augment.OnNodeEntry(node);
        }
    }

    private HeroInstance FindHeroInstanceByData(HeroData heroData)
    {
        foreach (var hero in GetOccupiedHeroes())
        {
            if (hero != null && hero.Data == heroData)
                return hero;
        }

        return null;
    }

    private void TryEvolveHero(HeroInstance heroInstance, bool includeStartingPerks = true)
    {
        if (heroInstance == null || heroInstance.Data == null) return;
        if (heroInstance.EvolutionCopies < 3) return;
        if (heroInstance.Data.NextEvolution == null) return;

        HeroData baseData = heroInstance.Data;
        HeroData evolvedData = baseData.NextEvolution;

        Debug.Log($"[GameManager] Evolution triggered: {baseData.name} -> {evolvedData.name}");

        // Remove old deck/cards from base form
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

        Debug.Log($"[GameManager] Removed base package for {baseData.name}: {removedCards} cards, {removedPerks} perks.");

        // Evolve the same unit in place
        heroInstance.EvolveTo(evolvedData, preserveHealthPercent: true);

        // Add evolved package
        if (evolvedData.Deck != null)
        {
            masterDeck.AddRange(evolvedData.Deck);
        }

        if (includeStartingPerks && evolvedData.StartingPerks != null)
        {
            masterPerks.AddRange(evolvedData.StartingPerks);
        }

        Debug.Log($"[GameManager] {baseData.name} evolved in place into {evolvedData.name}");
    }

    private bool TryPlaceHeroInFirstFreeSlot(HeroInstance heroInstance)
    {
        if (heroInstance == null) return false;

        EnsureSlotArrays();

        if (!IsValidHeroInstance(frontlineSlots[0]))
        {
            frontlineSlots[0] = heroInstance;
            heroInstance.Position = SlotPosition.Frontline;
            return true;
        }

        if (!IsValidHeroInstance(frontlineSlots[1]))
        {
            frontlineSlots[1] = heroInstance;
            heroInstance.Position = SlotPosition.Frontline;
            return true;
        }

        if (!IsValidHeroInstance(backlineSlots[0]))
        {
            backlineSlots[0] = heroInstance;
            heroInstance.Position = SlotPosition.Backline;
            return true;
        }

        if (!IsValidHeroInstance(backlineSlots[1]))
        {
            backlineSlots[1] = heroInstance;
            heroInstance.Position = SlotPosition.Backline;
            return true;
        }

        return false;
    }

    private void ClearAllHeroSlots()
    {
        EnsureSlotArrays();

        frontlineSlots[0] = null;
        frontlineSlots[1] = null;
        backlineSlots[0] = null;
        backlineSlots[1] = null;
    }

    private bool PlaceHeroInSpecificSlotIgnoringOccupancy(HeroInstance heroInstance, int slotIndex)
    {
        if (heroInstance == null) return false;

        EnsureSlotArrays();

        switch (slotIndex)
        {
            case 0:
                frontlineSlots[0] = heroInstance;
                heroInstance.Position = SlotPosition.Frontline;
                return true;

            case 1:
                frontlineSlots[1] = heroInstance;
                heroInstance.Position = SlotPosition.Frontline;
                return true;

            case 2:
                backlineSlots[0] = heroInstance;
                heroInstance.Position = SlotPosition.Backline;
                return true;

            case 3:
                backlineSlots[1] = heroInstance;
                heroInstance.Position = SlotPosition.Backline;
                return true;

            default:
                Debug.LogWarning($"[GameManager] Invalid slot index: {slotIndex}");
                return false;
        }
    }
}