using System;
using UnityEngine;

[Serializable]
public abstract class AugmentEffect
{
    public abstract void Execute();
    public virtual void OnNodeEntry(MapNode node) {}
}
