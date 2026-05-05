using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData; // Fallback/testing
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private MapEncounterDatabase encounterDatabase;

    [Header("Audio")]
    [SerializeField] private AudioClip normalBattleMusic;
    [SerializeField] private AudioClip bossMusic;

    private IEnumerator Start()
    {
        // Let the rest of the scene singletons/systems finish initializing first
        yield return null;

        // 0. Setup Music
        if (MusicManager.HasInstance && MapSystem.HasInstance && MapSystem.Instance.CurrentNode != null)
        {
            if (MapSystem.Instance.CurrentNode.NodeType == MapNodeType.BOSS)
            {
                if (bossMusic != null) MusicManager.Instance.PlayMusic(bossMusic);
            }
            else
            {
                if (normalBattleMusic != null) MusicManager.Instance.PlayMusic(normalBattleMusic);
            }
        }

        // 1. Setup Heroes from fixed slots
        List<HeroInstance> heroes = new List<HeroInstance>();

        if (GameManager.Instance != null)
        {
            heroes = GameManager.Instance.GetAllSlottedHeroes();
        }
        else if (heroData != null)
        {
            heroes = new List<HeroInstance>
            {
                new HeroInstance(heroData),
                null,
                null,
                null
            };
        }

        bool hasAtLeastOneHero = false;
        foreach (var hero in heroes)
        {
            if (hero != null && hero.Data != null)
            {
                hasAtLeastOneHero = true;
                break;
            }
        }

        if (!hasAtLeastOneHero)
        {
            Debug.LogError("[MatchSetupSystem] No HeroData found in slots! Ensure GameManager has slotted heroes or assign fallback heroData in Inspector.");
            yield break;
        }

        Debug.Log($"[MatchSetupSystem] Total slot entries passed to battle: {heroes.Count}");
        for (int i = 0; i < heroes.Count; i++)
        {
            string heroName = (heroes[i] != null && heroes[i].Data != null)
                ? heroes[i].Data.name
                : "EMPTY";

            Debug.Log($"[MatchSetupSystem] Slot {i}: {heroName}");
        }

        if (HeroSystem.Instance == null)
        {
            Debug.LogError("[MatchSetupSystem] HeroSystem.Instance is NULL.");
            yield break;
        }

        HeroSystem.Instance.Setup(heroes);

        // 2. Setup Enemies from Map Node
        List<EnemyData> activeEnemies = new List<EnemyData>();

        if (MapSystem.Instance != null && MapSystem.Instance.CurrentNode != null && encounterDatabase != null)
        {
            var pool = encounterDatabase.GetEnemiesForNode(MapSystem.Instance.CurrentNode.NodeType);
            if (pool != null && pool.Count > 0)
            {
                int count = UnityEngine.Random.Range(1, Mathf.Min(4, pool.Count + 1));
                for (int i = 0; i < count; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, pool.Count);
                    activeEnemies.Add(pool[randomIndex]);
                }

                Debug.Log($"[MatchSetupSystem] Randomly selected {activeEnemies.Count} enemies from pool of {pool.Count} for node: {MapSystem.Instance.CurrentNode.NodeType}");
            }
        }

        // Fallback to inspector list if no map pool was found
        if (activeEnemies.Count == 0 && enemyDatas != null && enemyDatas.Count > 0)
        {
            activeEnemies = new List<EnemyData>(enemyDatas);
            Debug.Log($"[MatchSetupSystem] Falling back to Inspector enemy list ({activeEnemies.Count} enemies)");
        }

        if (activeEnemies.Count == 0)
        {
            Debug.LogWarning("[MatchSetupSystem] No enemies found from encounterDatabase or fallback enemyDatas.");
        }

        if (EnemySystem.Instance == null)
        {
            Debug.LogError("[MatchSetupSystem] EnemySystem.Instance is NULL.");
            yield break;
        }

        EnemySystem.Instance.Setup(activeEnemies);

        // 3. Setup Deck
        List<CardData> combinedDeck = new List<CardData>();

        if (GameManager.Instance != null)
        {
            combinedDeck = new List<CardData>(GameManager.Instance.MasterDeck);
        }

        Debug.Log($"[MatchSetupSystem] MasterDeck count before battle setup: {combinedDeck.Count}");

        if (CardSystem.Instance == null)
        {
            Debug.LogError("[MatchSetupSystem] CardSystem.Instance is NULL.");
            yield break;
        }

        CardSystem.Instance.Setup(combinedDeck);

        // 4. Setup Perks
        if (PerkSystem.Instance == null)
        {
            Debug.LogError("[MatchSetupSystem] PerkSystem.Instance is NULL.");
            yield break;
        }

        if (GameManager.Instance != null)
        {
            foreach (var ownedPerkData in GameManager.Instance.MasterPerks)
            {
                PerkSystem.Instance.AddPerk(new Perk(ownedPerkData));
            }
        }
        else if (perkData != null)
        {
            PerkSystem.Instance.AddPerk(new Perk(perkData));
        }

        // 5. Start combat flow
        HeroTurnStartGA heroTurnStartGA = new();
        ActionSystem.Instance.Perform(heroTurnStartGA);
    }
}