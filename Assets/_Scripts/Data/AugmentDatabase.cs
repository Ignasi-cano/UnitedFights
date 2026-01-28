using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/AugmentDatabase")]
public class AugmentDatabase : ScriptableObject
{
    public List<AugmentData> SilverAugments;
    public List<AugmentData> GoldAugments;
    public List<AugmentData> PrismaticAugments;

    public List<AugmentData> GetPoolByTier(AugmentTier tier)
    {
        return tier switch
        {
            AugmentTier.SILVER => SilverAugments,
            AugmentTier.GOLD => GoldAugments,
            AugmentTier.PRISMATIC => PrismaticAugments,
            _ => SilverAugments
        };
    }
}
