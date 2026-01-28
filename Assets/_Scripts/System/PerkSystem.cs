using System.Collections.Generic;
using UnityEngine;

public class PerkSystem : Singleton<PerkSystem>
{
    [SerializeField] private PerksUI perksUI;
    private readonly List<Perk> perks = new();
    public void AddPerk(Perk perk)
    {
        Debug.Log($"[PerkSystem] Adding perk: {perk.Name}");
        perks.Add(perk);
        if (perksUI != null) perksUI.AddPerkUI(perk);
        perk.OnAdd();
    }
    public void RemovePerk(Perk perk)
    {
        perks.Remove(perk);
        if (perksUI != null) perksUI.RemovePerkUI(perk);
        perk.OnRemove();
    }

    public Perk GetPerk(PerkData data)
    {
        return perks.Find(p => p.Data == data);
    }
}
