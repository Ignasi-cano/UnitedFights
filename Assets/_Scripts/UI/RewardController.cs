using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RewardController : MonoBehaviour
{
    [SerializeField] private Transform rewardsContainer;
    [SerializeField] private RewardButton rewardButtonPrefab;
    [SerializeField] private int cardCount = 3;
    [SerializeField] private int goldAmount = 50;
    [SerializeField] private float perkChance = 0.1f;

    [Header("Icons")]
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private Sprite perkIcon;

    [Header("Data Databases")]
    [SerializeField] private List<CardData> cardPool;
    [SerializeField] private List<PerkData> perkPool;

    private bool canInteract = false;

    private void Start()
    {
        GenerateRewards();
        StartCoroutine(EnableInteraction());
    }

    private IEnumerator EnableInteraction()
    {
        yield return new WaitForSeconds(1.0f);
        canInteract = true;
    }

    private void GenerateRewards()
    {
        // Clear old rewards
        foreach (Transform child in rewardsContainer) Destroy(child.gameObject);

        // AUTO-FIX: Ensure horizontal layout fits everything
        var layout = rewardsContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = 10; // Reduce spacing
            layout.childControlWidth = false; // Don't force width
            layout.childForceExpandWidth = false;
        }

        // NEW: Check if this was an Elite or Boss for better rewards
        bool isEliteOrBoss = false;
        if (MapSystem.Instance != null && MapSystem.Instance.CurrentNode != null)
        {
            var type = MapSystem.Instance.CurrentNode.NodeType;
            isEliteOrBoss = (type == MapNodeType.ELITE || type == MapNodeType.BOSS);
        }

        // 1. Generate 3 Random Cards
        List<CardData> selectedCards = GetRandomItems(cardPool, cardCount);
        foreach (var card in selectedCards)
        {
            CreateRewardButton(card.Image, card.name, "Add to deck", () => 
            {
                if (!canInteract) return;
                GameManager.Instance.AddCardToMasterDeck(card);
                FinishRewards();
            });
        }

        // 2. Generate Gold Reward
        int actualGold = isEliteOrBoss ? goldAmount * 2 : goldAmount;
        CreateRewardButton(goldIcon, $"{actualGold} Gold", "Immediate income", () => 
        {
            if (!canInteract) return;
            CurrencySystem.Instance.AddGold(actualGold);
            FinishRewards();
        });

        // 3. Perk Reward
        // Elites/Bosses have 100% chance, others use perkChance
        float actualPerkChance = isEliteOrBoss ? 1.0f : perkChance;
        
        if (Random.value < actualPerkChance && perkPool.Count > 0)
        {
            PerkData randomPerk = perkPool[Random.Range(0, perkPool.Count)];
            Sprite icon = randomPerk.Image != null ? randomPerk.Image : perkIcon;
            
            CreateRewardButton(icon, randomPerk.name, "Permanent Power (Item)", () => 
            {
                if (!canInteract) return;
                GameManager.Instance.AddPerkToMasterPerks(randomPerk);
                FinishRewards();
            });
        }

        // 4. NEW: Add a Skip Button (The "option to not grab it")
        CreateRewardButton(null, "Skip / Proceed", "Continue without taking any reward", () => 
        {
            if (!canInteract) return;
            FinishRewards();
        });
    }

    private void CreateRewardButton(Sprite icon, string title, string desc, System.Action action)
    {
        RewardButton btn = Instantiate(rewardButtonPrefab, rewardsContainer);
        btn.Setup(icon, title, desc, action);
        
        // NEW: Reduce scale so more items fit on screen (+/- 0.8f is usually a good balance)
        btn.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
    }

    private List<T> GetRandomItems<T>(List<T> pool, int count)
    {
        List<T> result = new List<T>();
        if (pool == null || pool.Count == 0) return result;
        
        List<T> tempPool = new List<T>(pool);
        for (int i = 0; i < count && tempPool.Count > 0; i++)
        {
            int index = Random.Range(0, tempPool.Count);
            result.Add(tempPool[index]);
            tempPool.RemoveAt(index);
        }
        return result;
    }

    private void FinishRewards()
    {
        if (MapSystem.Instance != null && MapSystem.Instance.CurrentNode != null && MapSystem.Instance.CurrentNode.NodeType == MapNodeType.BOSS)
        {
            SceneManager.LoadScene("Victory");
        }
        else
        {
            SceneManager.LoadScene("MapScene");
        }
    }
}
