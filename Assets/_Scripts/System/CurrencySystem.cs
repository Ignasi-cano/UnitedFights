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
        
        SyncGoldToFirebase();
    }

    public bool TrySpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            OnGoldChanged?.Invoke();
            Debug.Log($"[CurrencySystem] Spent {amount} gold. Remaining: {gold}");
            
            if (ScoreSystem.HasInstance) ScoreSystem.Instance.AddGoldSpent(amount);
            
            SyncGoldToFirebase();
            return true;
        }
        
        Debug.LogWarning($"[CurrencySystem] Not enough gold! Have: {gold}, Need: {amount}");
        return false;
    }

    public void SetGold(int amount)
    {
        gold = amount;
        OnGoldChanged?.Invoke();
        SyncGoldToFirebase();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        RegisterPerformer();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        // CRITICAL FIX: Do NOT detach performer here.
        // Since this is a PersistentSingleton, duplicate instances (Impostors) will run OnDisable when destroyed.
        // If they detach the performer, the main surviving instance loses its connection.
        // We simply leave it attached. It's static, so it overwrites safely in OnEnable.
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene color, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        RegisterPerformer();
    }

    private void RegisterPerformer()
    {
        Debug.Log("[CurrencySystem] Registering GiveGoldGA performer...");
        ActionSystem.AttachPerformer<GiveGoldGA>(GiveGoldPerformer);
    }

    private System.Collections.IEnumerator GiveGoldPerformer(GiveGoldGA action)
    {
        Debug.Log($"[CurrencySystem] GiveGoldPerformer triggered for {action.Amount} gold.");
        AddGold(action.Amount);
        yield return null;
    }

    private void SyncGoldToFirebase()
    {
        if (ScoreManager.Instance != null && AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            string userId = AuthManager.Instance.CurrentUser.UserId;
            ScoreManager.Instance.AddToInventory(userId, "Currency", "Gold", gold);
        }
    }
}
