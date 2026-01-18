using System.Collections;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject damageVFX;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        Debug.Log("daño x1");
        foreach (var target in dealDamageGA.Targets)
        {
            if (target == null || target.IsDying) continue;

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
}
