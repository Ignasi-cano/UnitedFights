using UnityEngine;

public class HeroView : CombatantView
{
    public void Setup(HeroData heroData)
    {
        Debug.Log($"[HeroView] Setting up {heroData.name} with {heroData.Health} health.");
        SetupBase(heroData.Health, heroData.Image);
    }
}
