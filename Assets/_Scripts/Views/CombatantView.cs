using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Armor{get; private set;}
    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }
    private void UpdateHealthText()
    {
        healthText.text = "" + CurrentHealth ;
    }

    public void AddArmor(int armorAmount)
    {
        Armor += armorAmount;
    }

    public void Damage(int damageAmount)
    {
        if (Armor > 0)
        {
            if(Armor > damageAmount && Armor-damageAmount>0)
            {
                Armor -= damageAmount;
            }
            else
            {
              damageAmount -= Armor;
              CurrentHealth -= damageAmount;
            }
        }
        else
        {
           CurrentHealth -= damageAmount;
        }
        if (CurrentHealth <0)
            {
                CurrentHealth = 0;
            }  
        
        transform.DOShakePosition(0.2f,0.5f);
        UpdateHealthText();
    }
}
