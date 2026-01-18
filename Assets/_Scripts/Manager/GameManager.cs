using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    [SerializeField] private List<HeroData> availableHeroes = new List<HeroData>();
    public List<HeroData> AvailableHeroes => availableHeroes;
    [SerializeField] private List<HeroData> activeHeroes = new List<HeroData>();
    public List<HeroData> ActiveHeroes => activeHeroes;
    public const int MAX_HEROES = 3;

    // Master list of cards that persists between scenes
    private List<CardData> masterDeck = new List<CardData>();
    public List<CardData> MasterDeck => masterDeck;

    protected override void Awake()
    {
        base.Awake();
        
        // If we were destroyed by the Singleton pattern, don't continue
        if (Instance != this) return;

        Debug.Log($"[GameManager] Awake on {gameObject.name}. Hero count: {(availableHeroes != null ? availableHeroes.Count : 0)}");

        // Ensure we have at least one hero if none selected (for testing)
        if (activeHeroes.Count == 0 && availableHeroes != null && availableHeroes.Count > 0)
        {
            HeroData defaultHero = availableHeroes[0];
            activeHeroes.Add(defaultHero);
            if (defaultHero.Deck != null) masterDeck.AddRange(defaultHero.Deck);
            Debug.Log($"[GameManager] Added default hero: {defaultHero.name} and their cards to Master Deck.");
        }
    }

    public bool TryAddHero(HeroData hero)
    {
        // Check if we already have this hero type to avoid duplicate decks
        bool alreadyOwned = activeHeroes.Exists(h => h != null && h.name == hero.name);

        activeHeroes.Add(hero);
        Debug.Log($"[GameManager] Hero added to collection: {hero.name}. Total owned: {activeHeroes.Count}");
        
        // Add hero cards to the master deck ONLY if it's the first one of its kind
        if (!alreadyOwned && hero.Deck != null)
        {
            masterDeck.AddRange(hero.Deck);
            Debug.Log($"[GameManager] First time owning {hero.name}. Cards added to Master Deck.");
        }
        else if (alreadyOwned)
        {
            Debug.Log($"[GameManager] Already owned {hero.name}. Cards NOT added to Master Deck.");
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

    public void SelectHero(HeroData hero)
    {
        activeHeroes.Clear();
        masterDeck.Clear();
        
        activeHeroes.Add(hero);
        if (hero.Deck != null) masterDeck.AddRange(hero.Deck);
        
        Debug.Log($"Hero selected and set as active: {hero.name}. Initial cards added to Master Deck.");
    }
}
