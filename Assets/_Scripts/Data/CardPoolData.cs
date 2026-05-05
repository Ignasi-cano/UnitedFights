using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardPoolData", menuName = "United Fights/Card Pool")]
public class CardPoolData : ScriptableObject
{
    [System.Serializable]
    public class OwnerPool
    {
        public HeroData Owner;
        public List<CardData> Basic = new();
        public List<CardData> Common = new();
        public List<CardData> Uncommon = new();
        public List<CardData> Rare = new();
    }

    [SerializeField] private List<OwnerPool> ownerPools = new();

    public List<CardData> GetCards(HeroData owner, CardRarity rarity)
    {
        OwnerPool pool = ownerPools.Find(p => p.Owner == owner);
        if (pool == null) return new List<CardData>();

        return rarity switch
        {
            CardRarity.Basic => new List<CardData>(pool.Basic),
            CardRarity.Common => new List<CardData>(pool.Common),
            CardRarity.Uncommon => new List<CardData>(pool.Uncommon),
            CardRarity.Rare => new List<CardData>(pool.Rare),
            _ => new List<CardData>()
        };
    }

    public List<CardData> GetCardsForOwners(List<HeroData> owners, CardRarity rarity)
    {
        List<CardData> result = new();

        if (owners == null) return result;

        foreach (HeroData owner in owners)
        {
            if (owner == null) continue;
            result.AddRange(GetCards(owner, rarity));
        }

        return result;
    }
}