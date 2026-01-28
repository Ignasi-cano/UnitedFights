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
    public int PoisonIntensity { get; private set; }
    public bool IsDying { get; protected set; }

    // Internal tracker for other statuses
    private Dictionary<StatusEffectType, int> statusEffects = new();

    protected void SetupBase(int health, Sprite image)
    {
        Debug.Log($"[CombatantView] {gameObject.name} SetupBase called with health: {health}");
        MaxHealth = CurrentHealth = health;
        spriteRenderer.sprite = image;
        IsDying = false; 
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (statusEffectsUI != null)
        {
            statusEffectsUI.UpdateStatusEffectUI(StatusEffectType.HEALTH, CurrentHealth);
            // Hide redundant health text when the icon-based UI is active
            if (healthText != null) healthText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"[CombatantView] {gameObject.name} has NO StatusEffectsUI assigned!");
            
            // Fallback: Show the text if no icon UI is available
            if (healthText != null)
            {
                healthText.gameObject.SetActive(true);
                healthText.text = $"{CurrentHealth}/{MaxHealth}";
            }
        }
    }

    public void AddArmor(int armorAmount)
    {
        // Delegate to AddStatusEffect to centralize logic
        AddStatusEffect(StatusEffectType.ARMOR, armorAmount); 
        
        if (DamageNumbersSystem.HasInstance)
        {
            DamageNumbersSystem.Instance.Show(transform.position, $"+{armorAmount}", new Color(0.2f, 0.6f, 1f)); // Blue-ish for armor
        }
    }

    public void ResetArmor()
    {
        Armor = 0;
        if (statusEffectsUI != null)
        {
            statusEffectsUI.UpdateStatusEffectUI(StatusEffectType.ARMOR, 0);
        }
    }

    public void Damage(int damageAmount)
    {
        if (IsDying) return;
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

        if (damageAmount > 0)
        {
            if (DamageNumbersSystem.HasInstance)
            {
                Debug.Log($"[CombatantView] Requesting damage number for {damageAmount} at {transform.position}");
                DamageNumbersSystem.Instance.Show(transform.position, $"-{damageAmount}", Color.red);
            }
            else
            {
                Debug.LogWarning("[CombatantView] DamageNumbersSystem.Instance is MISSING!");
            }
        }

        // Clamp Health
        if (CurrentHealth < 0) CurrentHealth = 0;
         
        // Visual Feedback: Shake + Flash
        transform.DOShakePosition(0.2f, 0.4f);
        if (spriteRenderer != null)
        {
            spriteRenderer.DOColor(Color.red, 0.1f).OnComplete(() => spriteRenderer.DOColor(Color.white, 0.1f));
        }

        UpdateHealthText();

        // KO Visual Feedback
        if (CurrentHealth <= 0)
        {
            IsDying = true;
            if (this is HeroView)
            {
                spriteRenderer.DOFade(0.4f, 0.3f);
            }
        }
    }

    public void SetHealth(int health)
    {
        CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
        UpdateHealthText();
        UpdateStatusEffectIcons(); // Ensure visuals match
        
        if (CurrentHealth <= 0)
        {
            IsDying = true;
            if (this is HeroView)
            {
                spriteRenderer.DOFade(0.4f, 0.3f);
            }
        }
        else
        {
            IsDying = false; 
            spriteRenderer.DOFade(1f, 0.1f);
        }
    }

    private void UpdateStatusEffectIcons()
    {
        if (statusEffectsUI != null)
        {
            statusEffectsUI.UpdateStatusEffectUI(StatusEffectType.HEALTH, CurrentHealth);
        }
    }

    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        if (type == StatusEffectType.ARMOR)
        {
            Armor += stackCount;
            statusEffectsUI.UpdateStatusEffectUI(type, Armor);
            return;
        }

        // Initialize Poison Intensity if applying new poison
        if (type == StatusEffectType.POISON)
        {
             if (GetStatusEffectStacks(StatusEffectType.POISON) <= 0)
             {
                 PoisonIntensity = 2; // Default starting damage
             }
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
             if (type == StatusEffectType.POISON) PoisonIntensity = 0; // Reset
             return;
        }

        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            if (statusEffects[type] <= 0)
            {
                statusEffects.Remove(type);
                if (type == StatusEffectType.POISON) PoisonIntensity = 0; // Reset
            }
        }
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }

    public void MultiplyPoisonIntensity(int multiplier)
    {
        PoisonIntensity *= multiplier;
    }

    public int GetStatusEffectStacks(StatusEffectType type)
    {
        if (statusEffects.ContainsKey(type)) return statusEffects[type];
        return 0;
    }

    public void Heal(int amount)
    {
        if (IsDying && CurrentHealth <= 0) return; 
        SetHealth(CurrentHealth + amount);
    }

    public void ChangeMaxHealth(int delta)
    {
        MaxHealth += delta;
        CurrentHealth += delta; 
        SetHealth(CurrentHealth);
    }

    protected virtual void OnDestroy()
    {
        transform.DOKill();
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
        }
    }
}