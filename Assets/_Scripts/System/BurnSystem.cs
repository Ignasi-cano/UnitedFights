using System.Collections;
using UnityEngine;

public class BurnSystem : MonoBehaviour
{
    [SerializeField] private GameObject burnVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBurnGA>(ApplyBurnPerformer);
    }
        private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBurnGA>();
    }
    private IEnumerator ApplyBurnPerformer(ApplyBurnGA applyBurnGA)
    {
        CombatantView target = applyBurnGA.Target;
        if (target == null || target.IsDying) yield break;

        // Use DealDamageGA to ensure death logic is checked correctly in DamageSystem
        DealDamageGA dealDamageGA = new DealDamageGA(applyBurnGA.BurnDamage, new() { target }, null);
        ActionSystem.Instance.AddReaction(dealDamageGA);

        target.RemoveStatusEffect(StatusEffectType.BURN, 1);
        yield return null; 
    }

}
