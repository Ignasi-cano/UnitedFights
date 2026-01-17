using UnityEngine;
using System.Collections.Generic;
using System;

public class MatchSeetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData; // Re-added for fallback/testing
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private MapEncounterDatabase encounterDatabase;

    private void Start()
    {
        Debug.Log("Enviorement loaded");
        
        // 1. Setup Hero
        HeroData selectedHero = (GameManager.Instance != null && GameManager.Instance.SelectedHero != null) 
            ? GameManager.Instance.SelectedHero 
            : heroData;

        if (selectedHero == null)
        {
             Debug.LogError("No HeroData found! Ensure GameManager has a selected hero or Assign one in Inspector.");
             return;
        }
        HeroSystem.Instance.Setup(selectedHero);

        // 2. Setup Enemies from Map Node
        List<EnemyData> activeEnemies = new List<EnemyData>();
        if (MapSystem.Instance != null && MapSystem.Instance.CurrentNode != null && encounterDatabase != null)
        {
            var pool = encounterDatabase.GetEnemiesForNode(MapSystem.Instance.CurrentNode.NodeType);
            if (pool != null && pool.Count > 0)
            {
                // Randomly pick 1 to 3 enemies from the pool
                int count = UnityEngine.Random.Range(1, Mathf.Min(4, pool.Count + 1));
                for (int i = 0; i < count; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, pool.Count);
                    activeEnemies.Add(pool[randomIndex]);
                }
                Debug.Log($"Randomly selected {activeEnemies.Count} enemies from pool of {pool.Count} for node: {MapSystem.Instance.CurrentNode.NodeType}");
            }
        }

        // Fallback to inspector list if no map pool was found
        if (activeEnemies.Count == 0 && enemyDatas != null && enemyDatas.Count > 0)
        {
            activeEnemies = enemyDatas;
            Debug.Log($"Falling back to Inspector enemy list ({activeEnemies.Count} enemies)");
        }

        EnemySystem.Instance.Setup(activeEnemies);

        // 3. Setup Systems
        CardSystem.Instance.Setup(selectedHero.Deck);
        CardSystem.Instance.AddRandomCardToDeck();
        PerkSystem.Instance.AddPerk(new Perk(perkData));
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
