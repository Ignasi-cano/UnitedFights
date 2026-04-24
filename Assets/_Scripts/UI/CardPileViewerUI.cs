using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardPileViewerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform contentParent;
    [SerializeField] private CardListItemView cardItemPrefab;

public void Setup(string title, List<CardData> cards)
{
    if (titleText != null)
        titleText.text = $"{title} ({(cards != null ? cards.Count : 0)})";

    Debug.Log($"[CardPileViewerUI] Opening {title} with {(cards != null ? cards.Count : 0)} cards.");

    ClearContent();

    if (cards == null) return;

    foreach (CardData card in cards)
    {
        if (card == null) continue;

        CardListItemView item = Instantiate(cardItemPrefab, contentParent);
        item.Setup(card);
    }
}
    public void Close()
    {
        Destroy(gameObject);
    }

    private void ClearContent()
    {
        if (contentParent == null) return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}