using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NonCardRewardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(Sprite icon, string title, string description)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;
    }
}