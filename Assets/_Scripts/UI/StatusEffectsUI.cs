using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    [SerializeField] private Sprite armorSprite, burnSprite, healthSprite, poisonSprite;
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();
    public void UpdateStatusEffectUI(StatusEffectType statusEffectType, int stackCount)
    {
        if (this == null) return;

        if (stackCount == 0)
        {
             if(statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                statusEffectUIs.Remove(statusEffectType);
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            if (!statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                statusEffectUIs.Add(statusEffectType, statusEffectUI);
            }
            Sprite sprite = GetSpriteByType(statusEffectType);
            statusEffectUIs[statusEffectType].Set(sprite, stackCount);

        }
    }
    private Sprite GetSpriteByType(StatusEffectType statusEffectType)
    {
        Sprite sprite = statusEffectType switch
        {
            StatusEffectType.ARMOR => armorSprite,
            StatusEffectType.BURN => burnSprite,
            StatusEffectType.HEALTH => healthSprite,
            StatusEffectType.POISON => poisonSprite,
            _ => null,
        };

        if (sprite == null)
        {
            Debug.LogWarning($"StatusEffectsUI: Sprite for {statusEffectType} is NULL! Check Inspector assignments.");
        }
        return sprite;
    }
}
