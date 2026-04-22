using System;
using UnityEngine;

[Serializable]
public class HeroInstance
{
    public HeroData Data { get; private set; }
    public int CurrentHealth { get; set; }
    public int MaxHealthBonus { get; set; }
    public SlotPosition Position { get; set; }

    public HeroInstance(HeroData data)
    {
        Data = data;
        CurrentHealth = data.Health;
        MaxHealthBonus = 0;
    }

    public int GetMaxHealth()
    {
        return Data.Health + MaxHealthBonus;
    }
}
