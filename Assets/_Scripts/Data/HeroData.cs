using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/Hero")]

public class HeroData : ScriptableObject
{
    [SerializeField] private Sprite image;
    [SerializeField] private int health;
    [SerializeField] private StatusEffectsUI statusEffectUI;
    [SerializeField] private List<CardData> deck;

    public Sprite Image => image;
    public int Health => health;
    public StatusEffectsUI StatusEffectUI => statusEffectUI;
    public List<CardData> Deck => deck;
}
