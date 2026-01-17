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
        if (activeHeroes.Count >= MAX_HEROES)
        {
            Debug.LogWarning("[GameManager] Max heroes reached!");
            return false;
        }

        activeHeroes.Add(hero);
        Debug.Log($"[GameManager] Hero added: {hero.name}. Total: {activeHeroes.Count}");
        
        // Add hero cards to the master deck
        if (hero.Deck != null)
        {
            masterDeck.AddRange(hero.Deck);
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
