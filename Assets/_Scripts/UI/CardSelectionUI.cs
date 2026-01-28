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
    [SerializeField] private Vector3 cardScale = new Vector3(0.6f, 0.6f, 1f);

    private System.Action<CardData> onCardSelected;

    protected override void Awake()
    {
        base.Awake();
        // Remove self-deactivation here; the script depends on being activated manually.
        // If we deactivate it here, it will fight Open()'s SetActive(true) call.
        if (closeButton != null) closeButton.onClick.AddListener(() => panel.SetActive(false));
    }

    public void Open(string title, List<CardData> cards, System.Action<CardData> onSelected)
    {
        Debug.Log($"[CardSelectionUI] Open called with '{title}' and {cards?.Count} cards.");
        if (titleText != null) titleText.text = title;
        onCardSelected = onSelected;
        
        if (panel != null)
        {
            panel.SetActive(true);
            
            // NEW: Disable Horizontal Scrolling on the ScrollRect if it exists
            ScrollRect scrollRect = container.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
            }
        }
        else
        {
            Debug.LogError("[CardSelectionUI] Panel reference is missing in the inspector!");
        }
        
        Populate(cards);
    }

    private void Populate(List<CardData> cards)
    {
        if (container == null)
        {
            Debug.LogError("[CardSelectionUI] Container reference is missing!");
            return;
        }

        foreach (Transform child in container) Destroy(child.gameObject);

        if (cards == null || cards.Count == 0) return;

        foreach (var card in cards)
        {
            GameObject itemObj = Instantiate(cardSelectionItemPrefab, container);
            itemObj.transform.localScale = cardScale;
            
            // Use GetComponentInChildren to be consistent with ShopView and support varied prefab structures
            ShopItemUI itemUI = itemObj.GetComponentInChildren<ShopItemUI>(); 
            if (itemUI != null)
            {
                // UI Setup for selection: 0 cost since it's already paid at the shop
                itemUI.Setup(card.Image, card.name, 0, () => {
                    onCardSelected?.Invoke(card);
                    if (panel != null) panel.SetActive(false);
                });
            }
        }
    }
}
