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
            int burnStacks = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
            if(burnStacks > 0)
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
        if (attacker == null) yield break;

        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        yield return tween.WaitForCompletion();

        if (attacker == null) yield break; 

        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        HeroView targetHero = HeroSystem.Instance.GetRandomHeroView();
        if (targetHero != null)
        {
            DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { targetHero }, attackHeroGA.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
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

            // Load the Reward Scene after winning a battle
            SceneManager.LoadScene("RewardScene");
        }
    }
}
