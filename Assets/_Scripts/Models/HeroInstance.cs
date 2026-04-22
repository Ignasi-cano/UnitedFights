using System;
using UnityEngine;

[Serializable]
public class HeroInstance
{
    public HeroData Data { get; private set; }
    public int CurrentHealth { get; set; }
    public int MaxHealthBonus { get; set; }
    public SlotPosition Position { get; set; }

    // 1 = base obtenida, 2 = una copia extra, 3 = evoluciona
    public int EvolutionCopies { get; private set; } = 1;

    public HeroInstance(HeroData data)
    {
        Data = data;
        CurrentHealth = data.Health;
        MaxHealthBonus = 0;
        Position = SlotPosition.Frontline;
        EvolutionCopies = 1;
    }

    public int GetMaxHealth()
    {
        return Data.Health + MaxHealthBonus;
    }

    public void AddCopy()
    {
        EvolutionCopies++;
    }

    public void ResetCopies()
    {
        EvolutionCopies = 1;
    }

    public void EvolveTo(HeroData evolvedData, bool preserveHealthPercent = true)
    {
        if (evolvedData == null) return;

        float healthPercent = GetMaxHealth() > 0 ? (float)CurrentHealth / GetMaxHealth() : 1f;
        SlotPosition currentPosition = Position;
        int currentBonus = MaxHealthBonus;

        Data = evolvedData;
        MaxHealthBonus = currentBonus;
        Position = currentPosition;

        if (preserveHealthPercent)
        {
            CurrentHealth = Mathf.Clamp(
                Mathf.RoundToInt(GetMaxHealth() * healthPercent),
                1,
                GetMaxHealth()
            );
        }
        else
        {
            CurrentHealth = GetMaxHealth();
        }

        ResetCopies();
    }
}