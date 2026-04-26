using UnityEngine;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button acceptButton;

    [Header("Views")]
    [SerializeField] private GameObject cardRewardRoot;
    [SerializeField] private GameObject normalRewardRoot;
    [SerializeField] private CardListItemView cardListItemView;
    [SerializeField] private NonCardRewardView nonCardRewardView;

    private System.Action storedRewardAction;

    private void Awake()
    {
        if (acceptButton == null)
            acceptButton = GetComponentInChildren<Button>();

        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(ClaimReward);
        }
    }

    // Compatibility method for old scripts: AugmentSelectionUI, RandomEventUI, old RewardController.
    public void Setup(Sprite icon, string title, string description, System.Action onClaim)
    {
        SetupNonCard(icon, title, description, onClaim);
    }

    public void SetupNonCard(Sprite icon, string title, string description, System.Action onClaim)
    {
        storedRewardAction = onClaim;

        if (cardRewardRoot != null)
            cardRewardRoot.SetActive(false);

        if (normalRewardRoot != null)
            normalRewardRoot.SetActive(true);

        if (nonCardRewardView != null)
            nonCardRewardView.Setup(icon, title, description);
    }

    public void SetupCard(CardData card, System.Action onClaim)
    {
        storedRewardAction = onClaim;

        if (normalRewardRoot != null)
            normalRewardRoot.SetActive(false);

        if (cardRewardRoot != null)
            cardRewardRoot.SetActive(true);

        if (cardListItemView != null)
            cardListItemView.Setup(card);
    }

    private void ClaimReward()
    {
        storedRewardAction?.Invoke();
    }
}