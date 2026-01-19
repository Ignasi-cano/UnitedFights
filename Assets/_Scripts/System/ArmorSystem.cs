using System.Collections;
using UnityEngine;

public class ArmorSystem : Singleton<ArmorSystem>
{
    [SerializeField] private GameObject ArmorVFX;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<GainArmorGA>(GainArmorPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<GainArmorGA>();
    }
    private IEnumerator GainArmorPerformer(GainArmorGA gainArmorGA)
    {
        Debug.Log($"[ArmorSystem] Adding {gainArmorGA.Amount} armor to targets.");
        foreach (var target in gainArmorGA.Target)
        {
            if (target == null) continue;
            
            target.AddArmor(gainArmorGA.Amount);
            yield return new WaitForSeconds(0.15f);
        }
    }
}
