using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSystem : Singleton<PoisonSystem>
{
    [SerializeField] private GameObject poisonVFX;
    public int TotalPoisonDamageThisTurn { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyPoisonGA>(ApplyPoisonPerformer);
        ActionSystem.AttachPerformer<HealPoisonDamageGA>(HealPoisonDamagePerformer);
        // Reset on EnemyTurnGA (PRE) so we capture damage done during enemy turn for the player to leech off
        ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnStartReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyPoisonGA>();
        ActionSystem.DetachPerformer<HealPoisonDamageGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnStartReaction, ReactionTiming.PRE);
    }

    private void OnEnemyTurnStartReaction(EnemyTurnGA action)
    {
        TotalPoisonDamageThisTurn = 0;
        Debug.Log("[PoisonSystem] Reset poison damage tracker (EnemyTurn Start).");
    }

    private IEnumerator ApplyPoisonPerformer(ApplyPoisonGA applyPoisonGA)
    {
        CombatantView target = applyPoisonGA.Target;
        if (target == null || target.IsDying) yield break;

        int currentStacks = target.GetStatusEffectStacks(StatusEffectType.POISON);
        
        if (currentStacks > 0)
        {
            // 1. Deal damage equal to Poison Intensity (Damage Tracker)
            int damage = target.PoisonIntensity;
            TotalPoisonDamageThisTurn += damage;
            Debug.Log($"[PoisonSystem] Poison dealt {damage}. Total this turn: {TotalPoisonDamageThisTurn}");

            DealDamageGA dealDamageGA = new DealDamageGA(damage, new List<CombatantView> { target }, null);
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

    private IEnumerator HealPoisonDamagePerformer(HealPoisonDamageGA action)
    {
        Debug.Log($"[PoisonSystem] HealPoisonDamagePerformer STARTED. Total Poison Damage This Turn: {TotalPoisonDamageThisTurn}");
        
        int healAmount = TotalPoisonDamageThisTurn / 2;
        Debug.Log($"[PoisonSystem] Calculated Heal Amount: {healAmount} (Total / 2)");

        if (healAmount > 0)
        {
            if (action.Targets == null || action.Targets.Count == 0)
            {
                Debug.LogError("[PoisonSystem] HealPoisonDamageGA has NO TARGETS!");
            }
            else
            {
                Debug.Log($"[PoisonSystem] Healing {healAmount} to {action.Targets[0].name} based on poison damage.");
                foreach (var target in action.Targets)
                {
                    if (target != null)
                    {
                        target.Heal(healAmount);
                        Debug.Log($"[PoisonSystem] Heal called on {target.name}. Current HP: {target.CurrentHealth}");
                    }
                }
            }
        }
        else
        {
            Debug.Log("[PoisonSystem] No poison damage dealt this turn (or < 2), so no healing occurred.");
        }
        yield return null;
    }
}
