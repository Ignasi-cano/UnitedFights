using System.Collections.Generic;
using UnityEngine;

public class StatusEffectsUI : MonoBehaviour
{
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    [SerializeField] private Sprite armorSprite, burnSprite, healthSprite;
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();
    public void UpdateStatusEffectUI(StatusEffectType statusEffectType, int stackCount)
    {

        if (stackCount == 0)
        {
             if(statusEffectUIs.ContainsKey(statusEffectType))
            {
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType]; // Use a clear name
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
            _ => null,
        };

        if (sprite == null)
        {
            Debug.LogWarning($"StatusEffectsUI: Sprite for {statusEffectType} is NULL! Check Inspector assignments.");
        }
        return sprite;
    }
}
