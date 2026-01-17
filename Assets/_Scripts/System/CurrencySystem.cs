using UnityEngine;
using System;

public class CurrencySystem : PersistentSingleton<CurrencySystem>
{
    [SerializeField] private int gold;
    public int Gold => gold;

    public static event Action OnGoldChanged;

    public void AddGold(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke();
        Debug.Log($"[CurrencySystem] Added {amount} gold. Total: {gold}");
    }

    public bool TrySpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke();
            Debug.Log($"[CurrencySystem] Spent {amount} gold. Remaining: {gold}");
            return true;
        }
        
        Debug.LogWarning($"[CurrencySystem] Not enough gold! Have: {gold}, Need: {amount}");
        return false;
    }

    public void SetGold(int amount)
    {
        gold = amount;
        OnGoldChanged?.Invoke();
    }
}
