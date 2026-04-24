using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySystem : Singleton<EnemySystem>
{
    [SerializeField] private EnemyBoardView enemyBoardView;

    public List<EnemyView> Enemies => enemyBoardView.EnemyViews;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
        ActionSystem.SubscribeReaction<KillEnemyGA>(CheckEnemiesAlivePostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(CheckEnemiesAlivePostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(CheckEnemiesAlivePostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(CheckEnemiesAlivePostReaction, ReactionTiming.POST);
    }

    public void Setup(List<EnemyData> enemyDatas)
    {
        foreach (var enemyData in enemyDatas)
        {
            enemyBoardView.AddEnemy(enemyData);
        }
    }

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        ClearArmorGA clearArmorGA = new(new List<CombatantView>(Enemies));
        ActionSystem.Instance.AddReaction(clearArmorGA);

        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            if (enemy == null || enemy.IsDying || enemy.CurrentHealth <= 0) continue;

            int burnStacks = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
            if (burnStacks > 0)
            {
                ApplyBurnGA applyBurnGA = new(burnStacks, enemy);
                ActionSystem.Instance.AddReaction(applyBurnGA);
            }

            int poisonStacks = enemy.GetStatusEffectStacks(StatusEffectType.POISON);
            if (poisonStacks > 0)
            {
                ApplyPoisonGA applyPoisonGA = new(enemy);
                ActionSystem.Instance.AddReaction(applyPoisonGA);
            }

            AttackHeroGA attackHeroGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackHeroGA);
        }

        HeroTurnStartGA heroTurnStartGA = new();
        ActionSystem.Instance.AddReaction(heroTurnStartGA);

        yield return null;
    }

    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;
        if (attacker == null || attacker.IsDying || attacker.CurrentHealth <= 0) yield break;

        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();

        if (attacker == null || attacker.IsDying || attacker.CurrentHealth <= 0) yield break;

        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);

        List<CardData> pattern = attacker.EnemyData.AttackPattern;

        if (pattern != null && pattern.Count > 0)
        {
            CardData currentCard = pattern[attacker.PatternIndex % pattern.Count];
            attacker.PatternIndex++;

            if (currentCard.ManualTargetEffect != null)
            {
                List<CombatantView> targets = GetEnemyTargetsForEffect(
                    currentCard.ManualTargetEffect,
                    attacker,
                    isManualEffect: true
                );

                if (targets.Count > 0)
                {
                    PerformEffectsGA performEffectsGA = new(
                        currentCard.ManualTargetEffect,
                        targets,
                        attacker
                    );

                    ActionSystem.Instance.AddReaction(performEffectsGA);
                }
            }

            if (currentCard.OtherEffects != null)
            {
                foreach (var effectWrapper in currentCard.OtherEffects)
                {
                    if (effectWrapper == null || effectWrapper.Effect == null) continue;

                    List<CombatantView> targets = GetEnemyTargetsForEffect(
                        effectWrapper.Effect,
                        attacker,
                        isManualEffect: false,
                        targetMode: effectWrapper.TargetMode
                    );

                    if (targets.Count == 0) continue;

                    PerformEffectsGA performEffectGA = new(
                        effectWrapper.Effect,
                        targets,
                        attacker
                    );

                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }
        }
        else
        {
            CombatantView targetHero = attacker.CurrentManualIntentTarget;

            if (targetHero == null)
            {
                targetHero = HeroSystem.Instance.GetRandomFrontlineHero();
            }

            if (targetHero != null)
            {
                DealDamageGA dealDamageGA = new(
                    attacker.AttackPower,
                    new List<CombatantView> { targetHero },
                    attacker
                );

                ActionSystem.Instance.AddReaction(dealDamageGA);
            }
        }

        attacker.UpdateIntent();
    }

    private List<CombatantView> GetEnemyTargetsForEffect(
        Effect effect,
        EnemyView attacker,
        bool isManualEffect,
        TargetMode targetMode = null
    )
    {
        List<CombatantView> normalTargets = new();

        if (effect == null || attacker == null)
            return normalTargets;

        if (isManualEffect)
        {
            CombatantView targetHero = attacker.CurrentManualIntentTarget;

            if (targetHero == null)
            {
                targetHero = HeroSystem.Instance.GetRandomFrontlineHero();
            }

            if (targetHero != null)
            {
                normalTargets.Add(targetHero);
            }
        }
        else if (targetMode != null)
        {
            List<CombatantView> targetModeTargets = targetMode.GetTargets();

            if (targetModeTargets != null)
            {
                normalTargets.AddRange(targetModeTargets);
            }
        }

        if (EnemyEffectShouldSelfTarget(effect, normalTargets, attacker))
        {
            return new List<CombatantView> { attacker };
        }

        normalTargets.RemoveAll(target => target == null || target.IsDying || target.CurrentHealth <= 0);

        return normalTargets;
    }

    private bool EnemyEffectShouldSelfTarget(
        Effect effect,
        List<CombatantView> currentTargets,
        EnemyView attacker
    )
    {
        if (effect == null || attacker == null) return false;

        List<CombatantView> previewTargets =
            currentTargets != null && currentTargets.Count > 0
                ? new List<CombatantView>(currentTargets)
                : new List<CombatantView> { attacker };

        GameAction previewAction;

        try
        {
            previewAction = effect.GetGameAction(previewTargets, attacker);
        }
        catch
        {
            return false;
        }

        return previewAction is GainArmorGA || previewAction is HealGA;
    }

    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
    }

    private void CheckEnemiesAlivePostReaction(GameAction action)
    {
        if (Enemies.Count == 0)
        {
            Debug.Log("No enemies! Victory! Loading Reward Scene.");
            HeroSystem.Instance.SaveHeroesHealth();

            SceneManager.LoadScene("RewardScene");
        }
    }
}