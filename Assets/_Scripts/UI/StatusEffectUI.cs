using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class StatusEffectUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text stackCountText;

    public void Set(Sprite sprite, int stackCount)
    {
        // Debug Log removed
        image.sprite = sprite;

        // VISIBILITY ENFORCEMENT
        image.gameObject.SetActive(true);
        image.enabled = true;
        image.color = Color.white;

        // SIZE ENFORCEMENT
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(100, 100);
            rt.anchoredPosition = Vector2.zero;
        }

        stackCountText.text = stackCount.ToString();
    }
}
