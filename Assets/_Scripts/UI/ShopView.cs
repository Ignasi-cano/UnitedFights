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

    private void OnEnable()
    {
        CurrencySystem.OnGoldChanged += UpdateGoldUI;
        UpdateGoldUI();
        PopulateShop();
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

    public void OpenShop()
    {
        gameObject.SetActive(true);
    }

    public void PopulateShop()
    {
        // Clear existing items
        ClearContainer(heroContainer);
        ClearContainer(cardContainer);
        ClearContainer(perkContainer);

        // Populate Heroes
        foreach (var hero in heroPool)
        {
            CreateShopItem(cardPrefab, hero.Image, hero.name, hero.Cost, heroContainer, () => ShopSystem.Instance.BuyHero(hero));
        }

        // Populate Cards
        foreach (var card in cardPool)
        {
            CreateShopItem(cardPrefab, card.Image, card.name, card.Cost, cardContainer, () => ShopSystem.Instance.BuyCard(card));
        }

        // Populate Perks
        foreach (var perk in perkPool)
        {
            CreateShopItem(itemPrefab, perk.Image, perk.name, perk.Cost, perkContainer, () => ShopSystem.Instance.BuyPerk(perk));
        }
    }

    private void ClearContainer(Transform container)
    {
        if (container == null) return;
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateShopItem(GameObject prefab, Sprite icon, string name, int cost, Transform parent, System.Func<bool> onBuy)
    {
        if (parent == null || prefab == null) return;
        GameObject itemObj = Instantiate(prefab, parent);
        ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();
        if (itemUI != null)
        {
            itemUI.Setup(icon, name, cost, () =>
            {
                if (onBuy.Invoke())
                {
                    itemUI.SetSold();
                }
            });
        }
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}
