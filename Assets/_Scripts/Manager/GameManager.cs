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

    protected override void Awake()
    {
        base.Awake();
        
        // If we were destroyed by the Singleton pattern, don't continue
        if (Instance != this) return;

        Debug.Log($"[GameManager] Awake on {gameObject.name}. Hero count: {(availableHeroes != null ? availableHeroes.Count : 0)}");

        // Ensure we have at least one hero if none selected (for testing)
        if (activeHeroes.Count == 0 && availableHeroes != null && availableHeroes.Count > 0)
        {
            HeroData defaultHeroData = availableHeroes[0];
            HeroInstance defaultHero = new HeroInstance(defaultHeroData);
            activeHeroes.Add(defaultHero);
            if (defaultHeroData.Deck != null) masterDeck.AddRange(defaultHeroData.Deck);
            Debug.Log($"[GameManager] Added default hero: {defaultHeroData.name} and their cards to Master Deck.");
        }
    }

    public bool TryAddHero(HeroData heroData)
    {
        // Check if we already have this hero type to avoid duplicate decks
        bool alreadyOwned = activeHeroes.Exists(h => h != null && h.Data.name == heroData.name);

        HeroInstance heroInstance = new HeroInstance(heroData);
        activeHeroes.Add(heroInstance);
        Debug.Log($"[GameManager] Hero added to collection: {heroData.name}. Total owned: {activeHeroes.Count}");
        
        // Add hero cards to the master deck ONLY if it's the first one of its kind
        if (!alreadyOwned && heroData.Deck != null)
        {
            masterDeck.AddRange(heroData.Deck);
            Debug.Log($"[GameManager] First time owning {heroData.name}. Cards added to Master Deck.");
        }
        else if (alreadyOwned)
        {
            Debug.Log($"[GameManager] Already owned {heroData.name}. Cards NOT added to Master Deck.");
        }
        
        return true;
    }

    public void AddCardToMasterDeck(CardData card)
    {
        masterDeck.Add(card);
        Debug.Log($"[GameManager] Card added to Master Deck: {card.name}");
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
        
        HeroInstance heroInstance = new HeroInstance(heroData);
        activeHeroes.Add(heroInstance);
        if (heroData.Deck != null) masterDeck.AddRange(heroData.Deck);
        
        Debug.Log($"Hero selected and set as active: {heroData.name}. Initial cards added to Master Deck.");
    }
}
