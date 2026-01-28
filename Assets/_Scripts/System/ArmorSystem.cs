using System.Collections;
using UnityEngine;

public class ArmorSystem : Singleton<ArmorSystem>
{
    [SerializeField] private GameObject ArmorVFX;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<GainArmorGA>(GainArmorPerformer);
        ActionSystem.AttachPerformer<ClearArmorGA>(ClearArmorPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<GainArmorGA>();
        ActionSystem.DetachPerformer<ClearArmorGA>();
    }
    private IEnumerator GainArmorPerformer(GainArmorGA gainArmorGA)
    {
        Debug.Log($"[ArmorSystem] Adding {gainArmorGA.Amount} armor to {gainArmorGA.Target.Count} targets.");
        foreach (var target in gainArmorGA.Target)
        {
            if (target == null) 
            {
                Debug.LogWarning("[ArmorSystem] Target is NULL!");
                continue;
            }
            
            Debug.Log($"[ArmorSystem] Applying armor to {target.name}");
            target.AddArmor(gainArmorGA.Amount);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator ClearArmorPerformer(ClearArmorGA clearArmorGA)
    {
        Debug.Log($"[ArmorSystem] Clearing armor for {clearArmorGA.Targets.Count} targets.");
        foreach (var target in clearArmorGA.Targets)
        {
            if (target != null)
            {
                target.ResetArmor();
            }
        }
        yield return null;
    }
}
