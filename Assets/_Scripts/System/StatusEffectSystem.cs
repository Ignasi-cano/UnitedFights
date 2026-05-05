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
            if (target == null) continue;

            target.AddStatusEffect(addStatusEffectGa.StatusEffectType, addStatusEffectGa.StackCount);

            if (target is EnemyView enemyView)
            {
                enemyView.UpdateIntent();
            }

            yield return null;
        }
    }
}
