using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [field: SerializeField] public List<HeroData> AvailableHeroes { get; private set; }
    public HeroData SelectedHero { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        
        // Ensure we have a default hero if none selected (for testing)
        if (SelectedHero == null && AvailableHeroes.Count > 0)
        {
            SelectedHero = AvailableHeroes[0];
        }
    }

    public void SelectHero(HeroData hero)
    {
        SelectedHero = hero;
        Debug.Log($"Hero selected: {hero.name}");
    }
}
