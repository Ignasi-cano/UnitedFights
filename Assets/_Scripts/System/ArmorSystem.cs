using System.Collections;
using UnityEngine;

public class ArmorSystem : MonoBehaviour
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
        Debug.Log("escudo x1");
        foreach (var target in gainArmorGA.Target)
        {
            target.AddArmor(gainArmorGA.Amount);
            Instantiate(ArmorVFX, target.transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.15f);
        }
    }
}
