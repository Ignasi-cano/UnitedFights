using UnityEngine;
using SerializeReferenceEditor;

[CreateAssetMenu(menuName = "Data/Augment")]
public class AugmentData : ScriptableObject
{
    [SerializeField] private string augmentName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private AugmentTier tier;
    [field: SerializeReference, SR] public AugmentEffect Effect { get; private set; }

    public string Name => augmentName;
    public string Description => description;
    public Sprite Icon => icon;
    public AugmentTier Tier => tier;
}
