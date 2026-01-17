using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistentSingleton<GameManager>
{
    [SerializeField] private List<HeroData> availableHeroes = new List<HeroData>();
    public List<HeroData> AvailableHeroes => availableHeroes;
    public HeroData SelectedHero { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        
        // If we were destroyed by the Singleton pattern, don't continue
        if (Instance != this) return;

        Debug.Log($"[GameManager] Awake on {gameObject.name}. Hero count: {(availableHeroes != null ? availableHeroes.Count : 0)}");

        // Ensure we have a default hero if none selected (for testing)
        if (SelectedHero == null && availableHeroes != null && availableHeroes.Count > 0)
        {
            SelectedHero = availableHeroes[0];
            Debug.Log($"[GameManager] Set default hero: {SelectedHero.name}");
        }
    }

    public void SelectHero(HeroData hero)
    {
        SelectedHero = hero;
        Debug.Log($"Hero selected: {hero.name}");
    }
}
