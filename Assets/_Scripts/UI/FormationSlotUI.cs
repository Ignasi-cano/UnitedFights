using UnityEngine;
using UnityEngine.UI;
using System;

public class FormationSlotUI : MonoBehaviour
{
    [SerializeField] private Image heroIcon;
    [SerializeField] private Image selectionHighlight;
    [SerializeField] private int slotIndex;

    public int SlotIndex => slotIndex;

    private Action<FormationSlotUI> onSlotClicked;

    public void Setup(int index, HeroInstance hero, Action<FormationSlotUI> onClick)
    {
        slotIndex = index;
        onSlotClicked = onClick;

        if (hero != null && hero.Data != null)
        {
            heroIcon.sprite = hero.Data.Image;
            heroIcon.enabled = true;
        }
        else
        {
            heroIcon.enabled = false;
        }

        SetSelected(false);

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onSlotClicked?.Invoke(this));
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
            selectionHighlight.enabled = isSelected;
    }
}
