using TMPro;
using UnityEngine;

public class EnemyIntentUI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text valueText;

    public void Set(Sprite sprite, int value)
    {
        gameObject.SetActive(true);

        if (iconRenderer != null)
        {
            iconRenderer.sprite = sprite;
            iconRenderer.enabled = sprite != null;
        }

        if (valueText != null)
        {
            valueText.text = value.ToString();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}