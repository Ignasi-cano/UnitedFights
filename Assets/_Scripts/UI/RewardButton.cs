using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button button;

    public void Setup(Sprite icon, string name, string description, System.Action onClick)
    {
        if (iconImage != null) 
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
            if (icon == null) Debug.Log($"[RewardButton] Icon is null for {name}");
        }
        
        if (nameText != null) nameText.text = name;
        if (descriptionText != null) descriptionText.text = description;
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
