using System.Collections;
using UnityEngine;

public class StatusEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGa>(AddStatusEffectPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGa>();
    }
    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGa addStatusEffectGa)
    {
        foreach (var target in addStatusEffectGa.Targets)
        {
            target.AddStatusEffect(addStatusEffectGa.StatusEffectType, addStatusEffectGa.StackCount);
            yield return null; //add vfx for adding status effects
        }
    }
}
