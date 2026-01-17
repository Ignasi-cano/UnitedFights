using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardSelectionUI : Singleton<CardSelectionUI>
{
    [SerializeField] private GameObject cardSelectionItemPrefab; // Reuse shop item or create a specialized one
    [SerializeField] private Transform container;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;

    private System.Action<CardData> onCardSelected;

    private void Awake()
    {
        if (panel != null) panel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(() => panel.SetActive(false));
    }

    public void Open(string title, List<CardData> cards, System.Action<CardData> onSelected)
    {
        titleText.text = title;
        onCardSelected = onSelected;
        panel.SetActive(true);
        Populate(cards);
    }

    private void Populate(List<CardData> cards)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        foreach (var card in cards)
        {
            GameObject itemObj = Instantiate(cardSelectionItemPrefab, container);
            // Assuming we reuse ShopItemUI or something similar with icon/name
            ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>(); 
            if (itemUI != null)
            {
                // UI Setup for selection: 0 cost since it's already paid at the shop
                itemUI.Setup(card.Image, card.name, 0, () => {
                    onCardSelected?.Invoke(card);
                    panel.SetActive(false);
                });
            }
        }
    }
}
