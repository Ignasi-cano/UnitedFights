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
        ActionSystem.SubscribeReaction<EnemyTurnGA>(CheckEnemiesAlivePostReaction, ReactionTiming.POST);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        ActionSystem.DetachPerformer<AttackHeroGA>();
        ActionSystem.DetachPerformer<KillEnemyGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(CheckEnemiesAlivePostReaction, ReactionTiming.POST);
    }
    public void Setup(List<EnemyData> enemyDatas)
    {
        foreach (var enemyData in enemyDatas)
        {
            enemyBoardView.AddEnemy(enemyData);
        }
    }

    //performs

    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            int burnStacks = enemy.GetStatusEffectStacks(StatusEffectType.BURN);
            if(burnStacks > 0)
            {
                ApplyBurnGA applyBurnGA = new(burnStacks, enemy);
                ActionSystem.Instance.AddReaction(applyBurnGA);
            }
            AttackHeroGA attackHeroGA = new(enemy);
            ActionSystem.Instance.AddReaction(attackHeroGA);
        }
            yield return null;
    }
    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        EnemyView attacker = attackHeroGA.Attacker;

        // CHECK DE SEGURIDAD 1: 
        // Si el enemigo murió antes de que le tocara atacar (ej. por veneno o espinas), salimos.
        if (attacker == null) yield break;

        // Animación de ida (Hacia el héroe)
        // Guardamos el tween para poder verificar si sigue activo
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        
        // Esperamos a que llegue al frente
        yield return tween.WaitForCompletion();

        // CHECK DE SEGURIDAD 2 (EL MÁS IMPORTANTE):
        // Mientras esperábamos arriba, ¿el enemigo murió?
        if (attacker == null) yield break; 

        // Si sigue vivo, volvemos a la posición original
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);

        // Aplicamos el daño
        // Nota: Asegúrate de que HeroView tampoco sea null, por si acaso.
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

    private void CheckEnemiesAlivePostReaction(EnemyTurnGA enemyTurnGA)
    {
        if (Enemies.Count == 0)
        {
            Debug.Log("No enemies! Victory! Returning to Map.");
            SceneManager.LoadScene("MapScene");
        }
    }
}
