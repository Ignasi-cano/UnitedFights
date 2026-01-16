using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int Armor { get; private set; }

    // Internal tracker for other statuses
    private Dictionary<StatusEffectType, int> statusEffects = new();

    protected void SetupBase(int health, Sprite image)
    {
        MaxHealth = CurrentHealth = health;
        spriteRenderer.sprite = image;
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        // Optimization: ToString() creates less garbage than "" + int
        healthText.text = CurrentHealth.ToString(); 
    }

    public void AddArmor(int armorAmount)
    {
        // Delegate to AddStatusEffect to centralize logic
        AddStatusEffect(StatusEffectType.ARMOR, armorAmount); 
    }

    public void Damage(int damageAmount)
    {
        if (Armor > 0)
        {
            // Case 1: Armor absorbs all damage
            if (Armor >= damageAmount) 
            {
                Armor -= damageAmount;
                // Update UI: If armor remains, update count; if 0, remove it.
                if (Armor > 0) 
                    statusEffectsUI.UpdateStatusEffectUI(StatusEffectType.ARMOR, Armor);
                else 
                    RemoveStatusEffect(StatusEffectType.ARMOR, 0); // Clears icon

                damageAmount = 0; // Damage fully absorbed
            }
            // Case 2: Damage breaks armor and overflows to health
            else 
            {
                damageAmount -= Armor;
                Armor = 0; // LOGIC FIX: Armor must be destroyed
                RemoveStatusEffect(StatusEffectType.ARMOR, 0); // Clear Armor Icon

                CurrentHealth -= damageAmount;
            }
        }
        else
        {
            // Case 3: No Armor, direct health damage
            CurrentHealth -= damageAmount;
        }

        // Clamp Health
        if (CurrentHealth < 0) CurrentHealth = 0;
         
        // Visual Feedback
        transform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }

    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        if (type == StatusEffectType.ARMOR)
        {
            Armor += stackCount;
            statusEffectsUI.UpdateStatusEffectUI(type, Armor);
            return;
        }

        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] += stackCount;
        }
        else
        {
            statusEffects.Add(type, stackCount);
        }
        statusEffectsUI.UpdateStatusEffectUI(type, statusEffects[type]);
    }

    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        // If we are just clearing the icon (stackCount 0 or 'remove all'), handle it
        if(stackCount == 0 && statusEffects.ContainsKey(type))
        {
             statusEffects.Remove(type);
             statusEffectsUI.UpdateStatusEffectUI(type, 0);
             return;
        }

        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            if (statusEffects[type] <= 0)
            {
                statusEffects.Remove(type);
            }
        }
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }

    public int GetStatusEffectStacks(StatusEffectType type)
    {
        if (statusEffects.ContainsKey(type)) return statusEffects[type];
        return 0;
    }
}