using System.Collections.Generic;
using UnityEngine;
public class Card 
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public Effect ManualTargetEffect => data.ManualTargetEffect;
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects;
    public int Mana {get; private set; } 
    public CardData Data { get; private set; }
    public Card(CardData cardData)
    {
        Data = cardData;
        Mana = cardData.Mana;
    }
}