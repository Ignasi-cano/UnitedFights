using UnityEngine;
using SerializeReferenceEditor;
using Unity.VisualScripting;
using System.Collections.Generic;

[CreateAssetMenu(menuName ="Data/Perk")]
public class PerkData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set;}
    [field: SerializeReference, SR] public PerkCondition PerkCondition{ get; private set; }
    [field: SerializeReference, SR] public AutoTargetEffect AutoTargetEffect { get; private set; }
    [field: SerializeReference, SR] public List<Effect> OnAddEffects { get; private set; } = new();
    [field: SerializeField] public int Cost { get; private set; }
    [field: SerializeField] public bool UseAutoTarget {get; private set; } = true;
    [field: SerializeField] public bool UseActionCasterAsTarget {get; private set; } = false;
    [field: SerializeField] public bool UseActionTargets {get; private set; } = false;
}
