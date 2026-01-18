using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PerksUI : MonoBehaviour
{
    [SerializeField] private PerkUI perkUIPrefab;
    private readonly List<PerkUI> perkUIs = new();
    public void AddPerkUI(Perk perk)
    {
        if (perkUIPrefab == null)
        {
            Debug.LogError("[PerksUI] perkUIPrefab is null! Cannot add Perk UI.");
            return;
        }

        // Layout Check
        if (GetComponent<UnityEngine.UI.LayoutGroup>() == null)
        {
            Debug.LogWarning($"[PerksUI] {gameObject.name} has no LayoutGroup! Perks will overlap. Please add a Horizontal Layout Group.");
        }

        PerkUI perkUI = Instantiate(perkUIPrefab, transform);
        perkUI.Setup(perk);
        perkUIs.Add(perkUI);
    }
    public void RemovePerkUI(Perk perk)
    {
        PerkUI perkUI = perkUIs.Where(pui => pui.Perk == perk).FirstOrDefault();
        if (perkUI != null)
        {
            perkUIs.Remove(perkUI);
            Destroy(perkUI.gameObject);
        }
    }
}
