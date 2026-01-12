using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class HeroView : CombatantView
{
    public void Setup(HeroData heroData)
    {
        SetupBase(heroData.Health, heroData.Image);
    }
    
}
