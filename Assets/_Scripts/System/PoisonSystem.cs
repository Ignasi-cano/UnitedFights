using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSystem : MonoBehaviour
{
    [SerializeField] private GameObject poisonVFX;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyPoisonGA>(ApplyPoisonPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyPoisonGA>();
    }
    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA applyPoisonGA)
    {
        CombatantView target = applyPoisonGA.Target;
        if (target == null || target.IsDying) yield break;

        int currentStacks = target.GetStatusEffectStacks(StatusEffectType.POISON);
        
        if (currentStacks > 0)
        {
            // 1. Deal damage equal to Poison Intensity (Damage Tracker)
            DealDamageGA dealDamageGA = new DealDamageGA(target.PoisonIntensity, new List<CombatantView> { target }, null);
            ActionSystem.Instance.AddReaction(dealDamageGA);

            if (poisonVFX != null)
            {
                Instantiate(poisonVFX, target.transform.position, Quaternion.identity);
            }

            // 2. Double the INTENSITY (Damage for next turn)
            target.MultiplyPoisonIntensity(2);

            // 3. Decrement the DURATION (Stacks)
            target.RemoveStatusEffect(StatusEffectType.POISON, 1);
        }

        yield return null;
    }
}
