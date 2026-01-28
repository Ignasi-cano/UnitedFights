using System.Collections;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
        ActionSystem.AttachPerformer<HealGA>(HealPerformer);
        ActionSystem.AttachPerformer<ChangeMaxHealthGA>(ChangeMaxHealthPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
        ActionSystem.DetachPerformer<HealGA>();
        ActionSystem.DetachPerformer<ChangeMaxHealthGA>();
    }
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        Debug.Log("daño x1");
        foreach (var target in dealDamageGA.Targets)
        {
            if (target == null || target.IsDying) continue;

            if (dealDamageGA.Caster != null && dealDamageGA.Caster.GetStatusEffectStacks(StatusEffectType.BURN) > 0)
            {
                // Burn reduces damage by half
                dealDamageGA.Amount = Mathf.FloorToInt(dealDamageGA.Amount * 0.5f);
            }

            // --- BLACK CANDLE PENALTY ---
            // If the map node is ELITE or BOSS, and HERO is taking damage, increase it by 50%
            if (target is HeroView && HeroSystem.Instance.HasBlackCandle)
            {
                if (MapSystem.Instance.CurrentNode != null)
                {
                    var type = MapSystem.Instance.CurrentNode.NodeType;
                    if (type == MapNodeType.ELITE || type == MapNodeType.BOSS)
                    {
                        int extra = Mathf.FloorToInt(dealDamageGA.Amount * 0.5f);
                        dealDamageGA.Amount += extra;
                        Debug.Log($"[Black Candle] Penalty applied! +{extra} extra damage.");
                    }
                }
            }

            target.Damage(dealDamageGA.Amount);
            
            if (damageVFX != null)
            {
                Instantiate(damageVFX, target.transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.15f);
            
            // Re-check target because damage might have triggered death
            if (target != null && target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new(enemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    // Check if entire team is defeated
                    if (!HeroSystem.Instance.IsAnyHeroAlive)
                    {
                        Debug.Log("[DamageSystem] All heroes defeated! Game Over.");
                        ScoreSystem.Instance.SaveFinalScore();
                        UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
                    }
                    else
                    {
                        Debug.Log($"[DamageSystem] Hero {target.gameObject.name} KO'd, but others still standing.");
                    }
                }
                
            }
        }
    }
    private IEnumerator HealPerformer(HealGA healGA)
    {
        foreach (var target in healGA.Targets)
        {
            if (target == null) continue;
            target.Heal(healGA.Amount);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ChangeMaxHealthPerformer(ChangeMaxHealthGA changeMaxHealthGA)
    {
        foreach (var target in changeMaxHealthGA.Targets)
        {
            if (target == null) continue;
            target.ChangeMaxHealth(changeMaxHealthGA.Amount);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
