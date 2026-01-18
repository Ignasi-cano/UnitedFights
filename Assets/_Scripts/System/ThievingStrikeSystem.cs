using System.Collections;
using UnityEngine;

public class ThievingStrikeSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<ThievingStrikeGA>(ThievingStrikePerformer);
    }

    void OnDisable()
    {
        ActionSystem.DetachPerformer<ThievingStrikeGA>();
    }

    private IEnumerator ThievingStrikePerformer(ThievingStrikeGA action)
    {
        Debug.Log($"[ThievingStrikeSystem] Performing! Damage: {action.Damage}, Gold: {action.Gold}");
        // 1. Create and add DealDamageGA as a reaction
        DealDamageGA damageAction = new DealDamageGA(action.Damage, action.Targets, null);
        ActionSystem.Instance.AddReaction(damageAction);

        // 2. Create and add GiveGoldGA as a reaction
        GiveGoldGA goldAction = new GiveGoldGA(action.Gold);
        ActionSystem.Instance.AddReaction(goldAction);

        yield return null;
    }
}
