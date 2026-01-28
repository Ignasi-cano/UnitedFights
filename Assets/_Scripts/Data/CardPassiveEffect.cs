using UnityEngine;
using System;

[Serializable]
public abstract class CardPassiveEffect
{
    public abstract void Apply(); // For persistent changes
    public abstract int GetHandSizeModifier();
    public virtual void OnDraw(Card self) { }
}

[Serializable]
public class HandSizeModifierPassive : CardPassiveEffect
{
    [SerializeField] private int modifier = -1;

    public override void Apply() { } // Hand size is calculated dynamically
    public override int GetHandSizeModifier() => modifier;
}

[Serializable]
public class DiscardRandomOnDrawPassive : CardPassiveEffect
{
    public override void Apply() { }
    public override int GetHandSizeModifier() => 0;
    
    public override void OnDraw(Card self)
    {
        Debug.Log("[DiscardRandomOnDrawPassive] Triggered! Adding DiscardRandomCardGA reaction.");
        ActionSystem.Instance.AddReaction(new DiscardRandomCardGA());
    }
}
