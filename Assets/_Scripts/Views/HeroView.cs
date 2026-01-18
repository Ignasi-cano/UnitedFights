using UnityEngine;

public class HeroView : CombatantView
{
    public HeroInstance HeroInstance { get; private set; }
    public void Setup(HeroInstance instance)
    {
        HeroInstance = instance;
        Debug.Log($"[HeroView] Setting up {instance.Data.name} with {instance.CurrentHealth}/{instance.GetMaxHealth()} health.");
        SetupBase(instance.GetMaxHealth(), instance.Data.Image);
        
        // Restore saved health
        if (instance.CurrentHealth < instance.GetMaxHealth())
        {
            SetHealth(instance.CurrentHealth);
        }
    }
}
