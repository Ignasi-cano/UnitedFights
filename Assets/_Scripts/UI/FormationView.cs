using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FormationView : MonoBehaviour
{
    private static FormationView _instance;
    public static FormationView Instance
    {
        get
        {
            if (_instance == null)
                _instance = UnityEngine.Object.FindObjectOfType<FormationView>(true);
            return _instance;
        }
    }

    [SerializeField] private FormationSlotUI[] slots;
    [SerializeField] private Button closeButton;

    private FormationSlotUI selectedSlot;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseFormation);
    }

    public void OpenFormation()
    {
        gameObject.SetActive(true);
        RefreshSlots();
        selectedSlot = null;
    }

    public void RefreshSlots()
    {
        if (GameManager.Instance == null || slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            HeroInstance hero = GameManager.Instance.GetHeroAtSlot(i);
            slots[i].Setup(i, hero, OnSlotClicked);
        }
    }

    private void OnSlotClicked(FormationSlotUI clickedSlot)
    {
        if (selectedSlot == null)
        {
            selectedSlot = clickedSlot;
            selectedSlot.SetSelected(true);
        }
        else if (selectedSlot == clickedSlot)
        {
            selectedSlot.SetSelected(false);
            selectedSlot = null;
        }
        else
        {
            GameManager.Instance.SwapHeroSlots(selectedSlot.SlotIndex, clickedSlot.SlotIndex);
            selectedSlot.SetSelected(false);
            selectedSlot = null;
            RefreshSlots();
        }
    }

    public void CloseFormation() => gameObject.SetActive(false);
}
