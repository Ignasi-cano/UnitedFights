using UnityEngine;
using System.Collections.Generic;
using System;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData; // Re-added for fallback/testing
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private MapEncounterDatabase encounterDatabase;

    private void Start()
    {
        // 1. Setup Hero
        List<HeroInstance> heroes = new List<HeroInstance>();
        if (GameManager.Instance != null && GameManager.Instance.ActiveHeroes.Count > 0)
        {
            heroes = GameManager.Instance.ActiveHeroes;
        }
        else if (heroData != null)
        {
            heroes.Add(new HeroInstance(heroData));
        }

        if (heroes.Count == 0)
        {
             Debug.LogError("[MatchSetupSystem] No HeroData found! Ensure GameManager has active heroes or Assign one in Inspector.");
             return;
        }

        Debug.Log($"[MatchSetupSystem] Total heroes in battle: {heroes.Count}");
        HeroSystem.Instance.Setup(heroes);

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
        List<CardData> combinedDeck = new List<CardData>(GameManager.Instance.MasterDeck);
        
        CardSystem.Instance.Setup(combinedDeck);
        
        // Example: Add a random card as a starting reward (Permanent)
        if (CardSystem.Instance.AvailableCards != null && CardSystem.Instance.AvailableCards.Count > 0)
        {
            CardData randomCard = CardSystem.Instance.AvailableCards[UnityEngine.Random.Range(0, CardSystem.Instance.AvailableCards.Count)];
            GameManager.Instance.AddCardToMasterDeck(randomCard);
        }
        
        // Refresh combined deck from MasterDeck and Setup
        combinedDeck = new List<CardData>(GameManager.Instance.MasterDeck);
        CardSystem.Instance.Setup(combinedDeck);
        
        // 4. Setup Perks
        if (GameManager.Instance != null)
        {
            foreach (var perkData in GameManager.Instance.MasterPerks)
            {
                PerkSystem.Instance.AddPerk(new Perk(perkData));
            }
        }
        else
        {
            PerkSystem.Instance.AddPerk(new Perk(perkData));
        }
        
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
