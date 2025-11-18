
using UnityEngine;

public class Card 
{
    public string title => data.name;
    public string description => data.Description;
    public Sprite image => data.Image;
    public int mana {get;private set;} 
    private readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
        mana = cardData.Mana;
    }
}
