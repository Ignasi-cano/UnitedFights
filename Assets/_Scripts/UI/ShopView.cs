using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopView : MonoBehaviour
{
    public static ShopView Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // Hide by default
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshShop);
        }
    }

    [Header("Pools / Templates")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform heroContainer;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Transform perkContainer;

    [Header("UI References")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button closeButton;

    [Header("Available Items (Testing/Config)")]
    [SerializeField] private List<HeroData> heroPool;
    [SerializeField] private List<CardData> cardPool;
    [SerializeField] private List<PerkData> perkPool;

    [Header("Refresh Settings")]
    [SerializeField] private Button refreshButton;
    [SerializeField] private TMP_Text refreshCostText;
    [SerializeField] private int refreshCost = 2;

    private void OnEnable()
    {
        CurrencySystem.OnGoldChanged += UpdateGoldUI;
        UpdateGoldUI();
        
        if (refreshCostText != null)
            refreshCostText.text = $"{refreshCost} Gold";
            
        PopulateShop();
    }

    private void RefreshShop()
    {
        if (CurrencySystem.Instance.TrySpendGold(refreshCost))
        {
            Debug.Log("[ShopView] Refreshing shop...");
            PopulateShop();
        }
        else
        {
            Debug.LogWarning("[ShopView] Not enough gold to refresh shop!");
        }
    }

    private void OnDisable()
    {
        CurrencySystem.OnGoldChanged -= UpdateGoldUI;
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {CurrencySystem.Instance.Gold}";
    }

    [Header("Special Services")]
    [SerializeField] private Sprite removalServiceIcon;
    [SerializeField] private int removalServiceCost = 25;

    public void OpenShop()
    {
        gameObject.SetActive(true);
    }

    [ContextMenu("Force Populate Shop")]
    public void PopulateShop()
    {
        // 1. Clear ALL containers first
        HashSet<Transform> uniqueContainers = new HashSet<Transform>();
        if (heroContainer != null) uniqueContainers.Add(heroContainer);
        if (cardContainer != null) uniqueContainers.Add(cardContainer);
        if (perkContainer != null) uniqueContainers.Add(perkContainer);

        foreach (var container in uniqueContainers)
        {
            ClearContainer(container);
        }

        if (heroPool == null || cardPool == null || perkPool == null) return;

        // 2. Perform Random Selection
        // Pick 3 Random Heroes (if available)
        List<HeroData> selectedHeroes = GetRandomSubset(heroPool, 3);
        foreach (var hero in selectedHeroes)
        {
            CreateShopItem(cardPrefab, hero.Image, hero.name, hero.Cost, heroContainer, () => ShopSystem.Instance.BuyHero(hero));
        }

        // Pick 5 Random Cards
        List<CardData> selectedCards = GetRandomSubset(cardPool, 5);
        foreach (var card in selectedCards)
        {
            CreateShopItem(cardPrefab, card.Image, card.name, card.Cost, cardContainer, () => ShopSystem.Instance.BuyCard(card));
        }

        // Pick 2 Random Perks
        List<PerkData> selectedPerks = GetRandomSubset(perkPool, 2);
        foreach (var perk in selectedPerks)
        {
            CreateShopItem(itemPrefab, perk.Image, perk.name, perk.Cost, perkContainer, () => ShopSystem.Instance.BuyPerk(perk));
        }

        // 3. Add Fixed Services (Card Removal)
        if (removalServiceIcon != null)
        {
            ShopItemUI serviceUI = null;
            serviceUI = CreateShopItem(itemPrefab, removalServiceIcon, "Remove Card", removalServiceCost, perkContainer, () => 
            {
                ShopSystem.Instance.BuyCardRemoval(removalServiceCost, () => {
                    if (serviceUI != null) serviceUI.SetSold();
                });
                return false; // Deferred purchase
            });
        }
    }

    private List<T> GetRandomSubset<T>(List<T> source, int count)
    {
        if (source == null || source.Count == 0) return new List<T>();
        
        List<T> copy = new List<T>(source);
        List<T> result = new List<T>();
        
        int actualCount = Mathf.Min(count, copy.Count);
        for (int i = 0; i < actualCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, copy.Count);
            result.Add(copy[randomIndex]);
            copy.RemoveAt(randomIndex);
        }
        
        return result;
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        // Use a reverse loop with DestroyImmediate to ensure they are gone RIGHT NOW
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(container.GetChild(i).gameObject);
        }
    }

    private ShopItemUI CreateShopItem(GameObject prefab, Sprite icon, string name, int cost, Transform parent, System.Func<bool> onBuy)
    {
        if (parent == null || prefab == null) return null;
        GameObject itemObj = Instantiate(prefab, parent);
        
        // Use GetComponentInChildren to be more resilient to different prefab structures
        ShopItemUI itemUI = itemObj.GetComponentInChildren<ShopItemUI>();
        
        if (itemUI != null)
        {
            Debug.Log($"[ShopView] Spawning item: {name}");
            itemUI.Setup(icon, name, cost, () =>
            {
                if (onBuy.Invoke())
                {
                    itemUI.SetSold();
                }
            });
        }
        else
        {
            Debug.LogError($"[ShopView] Prefab {prefab.name} is missing ShopItemUI component!");
        }

        return itemUI;
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
        if (MapSystem.HasInstance)
        {
            MapSystem.Instance.RefreshMap();
        }
    }
}
