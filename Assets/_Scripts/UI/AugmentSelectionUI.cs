using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AugmentSelectionUI : Singleton<AugmentSelectionUI>
{
    [SerializeField] private GameObject overlay;
    [SerializeField] private AugmentDatabase database;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private RewardButton cardPrefab;

    protected override void Awake()
    {
        base.Awake();
        overlay.SetActive(false);
    }

    public void Open()
    {
        Debug.Log("[AugmentSelectionUI] Open called.");
        gameObject.SetActive(true); 
        overlay.SetActive(true);
        transform.SetAsLastSibling(); // Bring to front of Canvas
        PopulateAugments();
    }

    private void PopulateAugments()
    {
        Debug.Log("[AugmentSelectionUI] Populating Augments...");
        foreach (Transform child in cardsContainer) Destroy(child.gameObject);

        if (database == null) 
        {
            Debug.LogError("[AugmentSelectionUI] Database is missing in Inspector!");
            return;
        }

        // Tier selection with fallback
        AugmentTier selectedTier = GetRandomTier();
        List<AugmentData> pool = database.GetPoolByTier(selectedTier);

        if (pool == null || pool.Count == 0)
        {
            Debug.LogWarning($"[AugmentSelectionUI] Tier {selectedTier} is empty! Trying fallbacks...");
            // Try Silver, then Gold, then Prismatic if the first choice was empty
            if (database.SilverAugments.Count > 0) pool = database.SilverAugments;
            else if (database.GoldAugments.Count > 0) pool = database.GoldAugments;
            else if (database.PrismaticAugments.Count > 0) pool = database.PrismaticAugments;
        }

        if (pool == null || pool.Count == 0)
        {
            Debug.LogError("[AugmentSelectionUI] All tiers in Database are empty! Nothing to show.");
            return;
        }

        List<AugmentData> selected = GetRandomItems(pool, 3);
        Debug.Log($"[AugmentSelectionUI] Spawning {selected.Count} augment cards.");

        foreach (var augment in selected)
        {
            if (cardPrefab == null)
            {
                Debug.LogError("[AugmentSelectionUI] Card Prefab is missing in Inspector!");
                break;
            }

            RewardButton btn = Instantiate(cardPrefab, cardsContainer);
            
            // Format tier name for display
            string tierName = augment.Tier.ToString();
            string colorTag = augment.Tier switch
            {
                AugmentTier.SILVER => "<color=#C0C0C0>",
                AugmentTier.GOLD => "<color=#FFD700>",
                AugmentTier.PRISMATIC => "<color=#E0FFFF>",
                _ => "<color=white>"
            };

            string title = $"{colorTag}{tierName}</color>\n{augment.Name}";
            
            btn.Setup(augment.Icon, title, augment.Description, () => 
            {
                ApplyAugment(augment);
                Close();
            });
        }
    }

    private void ApplyAugment(AugmentData augment)
    {
        Debug.Log($"[AugmentSelectionUI] Applied Augment: {augment.Name}");
        if (augment.Effect != null)
        {
            GameManager.Instance.AddAugment(augment.Effect);
        }
    }

    private AugmentTier GetRandomTier()
    {
        float r = Random.value;
        if (r < 0.6f) return AugmentTier.SILVER;
        if (r < 0.9f) return AugmentTier.GOLD;
        return AugmentTier.PRISMATIC;
    }

    private void Close()
    {
        Debug.Log("[AugmentSelectionUI] Closing UI.");
        overlay.SetActive(false);
        gameObject.SetActive(false); // Disable the whole UI tree

        if (MapSystem.HasInstance)
        {
            MapSystem.Instance.RefreshMap();
        }
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
}
