using System.Collections.Generic;
using UnityEngine;

public class CardPileViewerManager : MonoBehaviour
{
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private CardPileViewerUI viewerPrefab;

    public void OpenDrawPile()
    {
        Open("Mazo de robo", CardSystem.Instance.GetDrawPileDataSortedByRarity());
    }

    public void OpenDiscardPile()
    {
        Open("Pila de descarte", CardSystem.Instance.GetDiscardPileDataInOrder());
    }

    public void OpenExilePile()
    {
        Open("Cartas exiliadas", CardSystem.Instance.GetExilePileDataInOrder());
    }

    public void OpenFullDeck()
    {
        Open("Deck completo", new List<CardData>(GameManager.Instance.MasterDeck));
    }

    private void Open(string title, List<CardData> cards)
    {
        if (targetCanvas == null)
        {
            Debug.LogError("[CardPileViewerManager] Target Canvas is not assigned.");
            return;
        }

        if (viewerPrefab == null)
        {
            Debug.LogError("[CardPileViewerManager] Viewer Prefab is not assigned.");
            return;
        }

        CardPileViewerUI viewer = Instantiate(viewerPrefab, targetCanvas.transform, false);

        RectTransform rect = viewer.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        viewer.transform.SetAsLastSibling();
        viewer.Setup(title, cards);
    }
}