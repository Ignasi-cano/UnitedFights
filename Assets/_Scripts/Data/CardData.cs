using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }

    [Header("Ownership")]
    [field: SerializeField] public HeroData OwnerHero { get; private set; }

    [Header("Classification")]
    [field: SerializeField] public CardType Type { get; private set; } = CardType.SKILL;
    [field: SerializeField] public CardRarity Rarity { get; private set; } = CardRarity.Basic;

    [Tooltip("If true, this ATTACK card can be played from Backline.")]
    [field: SerializeField] public bool HasDistanceKeyword { get; private set; } = false;

    [Header("Gameplay")]
    [field: SerializeField] public int Mana { get; private set; }
    [field: SerializeField] public bool IsCursed { get; private set; }
    [field: SerializeField] public int Cost { get; private set; }

    [Header("Intent UI")]
    [field: SerializeField] public Sprite IntentSprite { get; private set; }
    [field: SerializeField] public int IntentValue { get; private set; }

    [Header("Effects")]
    [field: SerializeReference, SR] public Effect ManualTargetEffect { get; private set; } = null;
    [field: SerializeField] public List<AutoTargetEffect> OtherEffects { get; private set; } = new();
    [field: SerializeReference, SR] public List<CardPassiveEffect> PassiveEffects { get; private set; } = new();

    public bool IsAttack => Type == CardType.ATTACK;
    public bool IsSkill => Type == CardType.SKILL;
    public bool IsPower => Type == CardType.POWER;

    public bool IsBasic => Rarity == CardRarity.Basic;

    public bool RequiresFrontline()
    {
        return Type == CardType.ATTACK && !HasDistanceKeyword;
    }
}