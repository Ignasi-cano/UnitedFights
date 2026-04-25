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

        if (pool == null)
        {
            Debug.LogWarning($"[CardPoolData] No pool found for owner: {(owner != null ? owner.name : "BASIC")}");
            return new List<CardData>();
        }

        return rarity switch
        {
            CardRarity.Basic => pool.Basic,
            CardRarity.Common => pool.Common,
            CardRarity.Uncommon => pool.Uncommon,
            CardRarity.Rare => pool.Rare,
            _ => new List<CardData>()
        };
    }

    public List<CardData> GetAllCardsForOwner(HeroData owner)
    {
        OwnerPool pool = ownerPools.Find(p => p.Owner == owner);

        if (pool == null)
            return new List<CardData>();

        List<CardData> result = new();
        result.AddRange(pool.Basic);
        result.AddRange(pool.Common);
        result.AddRange(pool.Uncommon);
        result.AddRange(pool.Rare);

        return result;
    }
}