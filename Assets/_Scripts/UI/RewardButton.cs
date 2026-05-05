using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button acceptButton;

    [Header("Views")]
    [SerializeField] private GameObject cardRewardRoot;
    [SerializeField] private GameObject normalRewardRoot;
    [SerializeField] private CardListItemView cardListItemView;
    [SerializeField] private NonCardRewardView nonCardRewardView;

    [Header("Shop Optional")]
    [SerializeField] private GameObject priceRoot;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Image coinIcon;

    private System.Action storedAction;

    private void Awake()
    {
        if (acceptButton == null)
            acceptButton = GetComponentInChildren<Button>();

        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        storedAction?.Invoke();
    }

    // Compatibility with old reward/event scripts
    public void Setup(Sprite icon, string title, string description, System.Action action)
    {
        SetupNonCard(icon, title, description, action);
    }

    public void SetupCard(CardData card, System.Action action)
    {
        storedAction = action;

        if (cardRewardRoot != null) cardRewardRoot.SetActive(true);
        if (normalRewardRoot != null) normalRewardRoot.SetActive(false);

        HidePrice();

        if (cardListItemView != null)
            cardListItemView.Setup(card);
    }

    public void SetupNonCard(Sprite icon, string title, string description, System.Action action)
    {
        storedAction = action;

        if (cardRewardRoot != null) cardRewardRoot.SetActive(false);
        if (normalRewardRoot != null) normalRewardRoot.SetActive(true);

        HidePrice();

        if (nonCardRewardView != null)
            nonCardRewardView.Setup(icon, title, description);
    }

    public void SetupCardShop(CardData card, int price, System.Action action)
    {
        SetupCard(card, action);
        ShowPrice(price);
    }

    public void SetupNonCardShop(Sprite icon, string title, string description, int price, System.Action action)
    {
        SetupNonCard(icon, title, description, action);
        ShowPrice(price);
    }

    private void ShowPrice(int price)
    {
        if (priceRoot != null)
            priceRoot.SetActive(true);

        if (priceText != null)
            priceText.text = price.ToString();

        if (coinIcon != null)
            coinIcon.gameObject.SetActive(true);
    }

    private void HidePrice()
    {
        if (priceRoot != null)
            priceRoot.SetActive(false);

        if (coinIcon != null)
            coinIcon.gameObject.SetActive(false);
    }
}