using System.Collections.Generic;
using UnityEngine;
public class Card 
{
    public string Title => Data.name;
    public string Description => Data.Description;
    public Sprite Image => Data.Image;
    public Effect ManualTargetEffect => Data.ManualTargetEffect;
    public List<AutoTargetEffect> OtherEffects => Data.OtherEffects;
    public int Mana {get; private set; } 
    public CardData Data { get; private set; }
    public Card(CardData cardData)
    {
        Data = cardData;
        Mana = cardData.Mana;
    }
}