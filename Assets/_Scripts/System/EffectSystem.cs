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
    //performers
    private IEnumerator PerformEffectPerformer(PerformEffectsGA performEffectsGA)
    {
        GameAction effectAction = performEffectsGA.Effect.GetGameAction(performEffectsGA.Targets, HeroSystem.Instance.HeroView);
        ActionSystem.Instance.AddReaction(effectAction);
        yield return null;
    }
}
