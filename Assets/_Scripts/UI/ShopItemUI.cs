using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private GameObject soldStateObject; // e.g. a "SOLD" stamp image or text

    public void Setup(Sprite icon, string itemName, int cost, System.Action onBuy)
    {
        if (iconImage != null) iconImage.sprite = icon;
        if (nameText != null) nameText.text = itemName;
        if (costText != null) costText.text = cost.ToString();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy?.Invoke());
        
        if (soldStateObject != null) soldStateObject.SetActive(false);
        buyButton.interactable = true;
    }

    public void SetSold()
    {
        buyButton.interactable = false;
        if (soldStateObject != null) soldStateObject.SetActive(true);
        costText.text = "---";
    }
}
