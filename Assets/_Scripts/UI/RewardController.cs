using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RewardController : MonoBehaviour
{
    [SerializeField] private Transform rewardsContainer;
    [SerializeField] private RewardButton rewardButtonPrefab;

    [Header("Reward Amounts")]
    [SerializeField] private int cardRewardCount = 3;
    [SerializeField] private int goldAmount = 50;
    [SerializeField] private float perkChance = 0.1f;

    [Header("Icons")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite perkIcon;

    [Header("Databases")]
    [SerializeField] private CardPoolData cardPoolData;
    [SerializeField] private List<PerkData> perkPool;

    [Header("Rarity Chances")]
    [SerializeField] private float commonChance = 0.65f;
    [SerializeField] private float uncommonChance = 0.25f;
    [SerializeField] private float rareChance = 0.10f;

    private bool canInteract = false;

    private void Start()
    {
        GenerateRewards();
        StartCoroutine(EnableInteraction());
    }

    private IEnumerator EnableInteraction()
    {
        yield return new WaitForSeconds(0.5f);
        canInteract = true;
    }

    private void GenerateRewards()
    {
        ClearRewardContainer();

        HorizontalLayoutGroup layout = rewardsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 10;
            layout.childControlWidth = false;
            layout.childForceExpandWidth = false;
        }

        bool isEliteOrBoss = IsEliteOrBossNode();

        List<CardData> cards = GenerateCardRewards(cardRewardCount);

        foreach (CardData card in cards)
        {
            CreateCardRewardButton(card, () =>
            {
                if (!canInteract) return;

                GameManager.Instance.AddCardToMasterDeck(card);
                FinishRewards();
            });
        }

        int actualGold = isEliteOrBoss ? goldAmount * 2 : goldAmount;

        CreateNormalRewardButton(
            goldIcon,
            $"{actualGold} Gold",
            "Immediate income.",
            () =>
            {
                if (!canInteract) return;

                CurrencySystem.Instance.AddGold(actualGold);
                FinishRewards();
            }
        );

        float actualPerkChance = isEliteOrBoss ? 1f : perkChance;

        if (Random.value < actualPerkChance && perkPool != null && perkPool.Count > 0)
        {
            PerkData randomPerk = perkPool[Random.Range(0, perkPool.Count)];
            Sprite icon = randomPerk.Image != null ? randomPerk.Image : perkIcon;

            CreateNormalRewardButton(
                icon,
                randomPerk.name,
                "Permanent power.",
                () =>
                {
                    if (!canInteract) return;

                    GameManager.Instance.AddPerkToMasterPerks(randomPerk);
                    FinishRewards();
                }
            );
        }

        CreateNormalRewardButton(
            null,
            "Skip",
            "Continue without taking a reward.",
            () =>
            {
                if (!canInteract) return;

                FinishRewards();
            }
        );
    }

    private void ClearRewardContainer()
    {
        if (rewardsContainer == null) return;

        foreach (Transform child in rewardsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateCardRewardButton(CardData card, System.Action action)
    {
        RewardButton btn = Instantiate(rewardButtonPrefab, rewardsContainer);
        btn.SetupCard(card, action);
    }

    private void CreateNormalRewardButton(Sprite icon, string title, string description, System.Action action)
    {
        RewardButton btn = Instantiate(rewardButtonPrefab, rewardsContainer);
        btn.Setup(icon, title, description, action);
    }

    private List<CardData> GenerateCardRewards(int amount)
    {
        List<CardData> rewards = new();
        List<CardData> alreadyPicked = new();

        if (cardPoolData == null)
        {
            Debug.LogError("[RewardController] CardPoolData is not assigned.");
            return rewards;
        }

        List<HeroData> validOwners = GetEligibleCardOwnersFromTeam();

        for (int i = 0; i < amount; i++)
        {
            CardRarity rarity = RollRewardRarity();

            List<CardData> candidates = cardPoolData.GetCardsForOwners(validOwners, rarity);
            candidates.RemoveAll(card => card == null || alreadyPicked.Contains(card));

            if (candidates.Count == 0)
            {
                candidates = GetFallbackCards(validOwners, alreadyPicked);
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("[RewardController] No valid card rewards found.");
                break;
            }

            CardData chosen = candidates[Random.Range(0, candidates.Count)];
            rewards.Add(chosen);
            alreadyPicked.Add(chosen);
        }

        return rewards;
    }

    private List<CardData> GetFallbackCards(List<HeroData> validOwners, List<CardData> alreadyPicked)
    {
        List<CardData> result = new();

        result.AddRange(cardPoolData.GetCardsForOwners(validOwners, CardRarity.Common));
        result.AddRange(cardPoolData.GetCardsForOwners(validOwners, CardRarity.Uncommon));
        result.AddRange(cardPoolData.GetCardsForOwners(validOwners, CardRarity.Rare));
        result.AddRange(cardPoolData.GetCardsForOwners(validOwners, CardRarity.Basic));

        result.RemoveAll(card => card == null || alreadyPicked.Contains(card));

        return result;
    }

    private CardRarity RollRewardRarity()
    {
        float roll = Random.value;

        if (roll < commonChance)
            return CardRarity.Common;

        if (roll < commonChance + uncommonChance)
            return CardRarity.Uncommon;

        return CardRarity.Rare;
    }

    private List<HeroData> GetEligibleCardOwnersFromTeam()
    {
        List<HeroData> owners = new();

        if (GameManager.Instance == null)
            return owners;

        foreach (HeroInstance hero in GameManager.Instance.ActiveHeroes)
        {
            if (hero == null || hero.Data == null) continue;

            HeroData current = hero.Data;

            while (current != null)
            {
                if (!owners.Contains(current))
                    owners.Add(current);

                current = current.PreviousEvolution;
            }
        }

        return owners;
    }

    private bool IsEliteOrBossNode()
    {
        if (MapSystem.Instance == null || MapSystem.Instance.CurrentNode == null)
            return false;

        MapNodeType type = MapSystem.Instance.CurrentNode.NodeType;

        return type == MapNodeType.ELITE || type == MapNodeType.BOSS;
    }

    private void FinishRewards()
    {
        if (MapSystem.Instance != null &&
            MapSystem.Instance.CurrentNode != null &&
            MapSystem.Instance.CurrentNode.NodeType == MapNodeType.BOSS)
        {
            SceneManager.LoadScene("Victory");
        }
        else
        {
            SceneManager.LoadScene("MapScene");
        }
    }
}