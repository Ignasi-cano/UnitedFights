using System.Collections;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectsGA>(PerformEffectPerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectsGA>();
    }

    private IEnumerator PerformEffectPerformer(PerformEffectsGA performEffectsGA)
    {
        if (performEffectsGA == null || performEffectsGA.Effect == null)
        {
            yield break;
        }

        CombatantView caster = performEffectsGA.Caster;

        if (caster == null)
        {
            caster = HeroSystem.Instance.MainHeroView;
        }

        GameAction effectAction = performEffectsGA.Effect.GetGameAction(
            performEffectsGA.Targets,
            caster
        );

        ActionSystem.Instance.AddReaction(effectAction);
        yield return null;
    }
}