using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class ScoreManager : Singleton<ScoreManager>
{
    private FirebaseFirestore db;
   
    public event Action<List<PlayerScore>> OnLeaderboardLoaded;

    private void Start()
    {
        if (FirebaseManager.Instance.IsInitialized)
        {
            InitializeFirestore();
        }
        else
        {
            FirebaseManager.Instance.OnFirebaseInitialized += InitializeFirestore;
        }
    }

    private void InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
        
        // Listen for logins to ensure legacy users get a document
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnLoginSuccess += EnsureUserDocumentExists;
            
            // If already logged in when initialized
            if (AuthManager.Instance.IsLoggedIn)
            {
                EnsureUserDocumentExists(AuthManager.Instance.CurrentUser);
            }
        }
    }

    private void EnsureUserDocumentExists(Firebase.Auth.FirebaseUser user)
    {
        if (db == null || user == null) return;

        DocumentReference userRef = db.Collection("users").Document(user.UserId);
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.Result.Exists)
            {
                Debug.Log($"[ScoreManager] Legacy user detected ({user.Email}). Creating Firestore document...");
                CreateUserDocument(user.UserId, user.Email);
            }
        });
    }

    private void OnDestroy()
    {
        if (AuthManager.HasInstance)
        {
            AuthManager.Instance.OnLoginSuccess -= EnsureUserDocumentExists;
        }
    }

    public void CreateUserDocument(string userId, string email)
    {
        if (db == null) return;

        Dictionary<string, object> userData = new()
        {
            { "email", email },
            { "highScore", 0 },
            { "gamesPlayed", 0 },
            { "elo", 1000 },
            { "tier", "Bronze" },
            { "createdAt", FieldValue.ServerTimestamp }
        };

        db.Collection("users").Document(userId).SetAsync(userData);
    }

    public void SaveScore(int score)
    {
        if (db == null || !AuthManager.Instance.IsLoggedIn) return;

        string userId = AuthManager.Instance.CurrentUser.UserId;
        DocumentReference userRef = db.Collection("users").Document(userId);

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                int currentHighScore = 0;
                if (task.Result.ContainsField("highScore"))
                    currentHighScore = task.Result.GetValue<int>("highScore");
                
                int gamesPlayed = 0;
                if (task.Result.ContainsField("gamesPlayed"))
                    gamesPlayed = task.Result.GetValue<int>("gamesPlayed");

                int currentElo = 1000;
                if (task.Result.ContainsField("elo"))
                    currentElo = task.Result.GetValue<int>("elo");

                // Simple progression: +20 per game
                int newElo = currentElo + 20;
                string newTier = GetTierFromElo(newElo);

                Dictionary<string, object> updates = new()
                {
                    { "gamesPlayed", gamesPlayed + 1 },
                    { "lastScore", score },
                    { "elo", newElo },
                    { "tier", newTier },
                    { "lastPlayedAt", FieldValue.ServerTimestamp }
                };

                // Only update highScore if it's higher
                if (score > currentHighScore)
                {
                    updates["highScore"] = score;
                }

                userRef.UpdateAsync(updates);
            }
        });
    }

    public void LoadLeaderboard(int limit = 10)
    {
        if (db == null) return;

        db.Collection("users")
            .OrderByDescending("highScore")
            .Limit(limit)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error loading leaderboard: " + task.Exception);
                    return;
                }

                List<PlayerScore> scores = new();
                foreach (DocumentSnapshot doc in task.Result.Documents)
                {
                    string email = doc.ContainsField("email") ? doc.GetValue<string>("email") : "Unknown";
                    int score = doc.ContainsField("highScore") ? doc.GetValue<int>("highScore") : 0;
                    int games = doc.ContainsField("gamesPlayed") ? doc.GetValue<int>("gamesPlayed") : 0;
                    int elo = doc.ContainsField("elo") ? doc.GetValue<int>("elo") : 1000;
                    string tier = doc.ContainsField("tier") ? doc.GetValue<string>("tier") : "Bronze";

                    scores.Add(new PlayerScore
                    {
                        Email = email,
                        HighScore = score,
                        GamesPlayed = games,
                        Elo = elo,
                        Tier = tier
                    });
                }

                OnLeaderboardLoaded?.Invoke(scores);
            });
    }

    public string GetTierFromElo(int elo)
    {
        if (elo < 1200) return "Bronze";
        if (elo < 1500) return "Silver";
        if (elo < 1800) return "Gold";
        if (elo < 2100) return "Platinum";
        return "Diamond";
    }
    public void AddMatchRecord(MatchRecord record)
    {
        if (db == null) return;
        db.Collection("matchHistory").AddAsync(record);
    }

    public void UpdateHeroStats(string userId, string heroName, bool won)
    {
        if (db == null) return;
        DocumentReference heroRef = db.Collection("herostats").Document($"{userId}_{heroName}");
        
        db.RunTransactionAsync(transaction =>
        {
            return transaction.GetSnapshotAsync(heroRef).ContinueWith(task =>
            {
                Dictionary<string, object> stats;
                if (task.Result.Exists)
                {
                    stats = task.Result.ToDictionary();
                    stats["gamesPlayed"] = Convert.ToInt32(stats["gamesPlayed"]) + 1;
                    if (won) stats["wins"] = Convert.ToInt32(stats["wins"]) + 1;
                }
                else
                {
                    stats = new Dictionary<string, object>
                    {
                        { "userId", userId },
                        { "heroName", heroName },
                        { "gamesPlayed", 1 },
                        { "wins", won ? 1 : 0 }
                    };
                }
                transaction.Set(heroRef, stats);
            });
        });
    }

    public void AddToInventory(string userId, string itemType, string itemId, int amount = 1)
    {
        if (db == null) return;
        DocumentReference itemRef = db.Collection("inventory").Document($"{userId}_{itemId}");
        
        Dictionary<string, object> itemData = new()
        {
            { "userId", userId },
            { "itemType", itemType },
            { "itemId", itemId },
            { "amount", amount },
            { "updatedAt", FieldValue.ServerTimestamp }
        };
        
        itemRef.SetAsync(itemData, SetOptions.MergeAll);
    }
}

[Serializable]
[FirestoreData]
public class PlayerScore
{
    [FirestoreProperty] public string Email { get; set; }
    [FirestoreProperty] public int HighScore { get; set; }
    [FirestoreProperty] public int GamesPlayed { get; set; }
    [FirestoreProperty] public int Elo { get; set; } = 1000; // Default starting Elo
    [FirestoreProperty] public string Tier { get; set; } = "Bronze";
}

[Serializable]
[FirestoreData]
public class MatchRecord
{
    [FirestoreProperty] public string WinnerId { get; set; }
    [FirestoreProperty] public string LoserId { get; set; }
    [FirestoreProperty] public int DamageDealt { get; set; }
    [FirestoreProperty] public string Timestamp { get; set; } = DateTime.UtcNow.ToString();
}

[Serializable]
[FirestoreData]
public class HeroStat
{
    [FirestoreProperty] public string UserId { get; set; }
    [FirestoreProperty] public string HeroName { get; set; }
    [FirestoreProperty] public int Wins { get; set; }
    [FirestoreProperty] public int GamesPlayed { get; set; }
}

[Serializable]
[FirestoreData]
public class InventoryItem
{
    [FirestoreProperty] public string UserId { get; set; }
    [FirestoreProperty] public string ItemType { get; set; } // "Card", "Skin", "Currency"
    [FirestoreProperty] public string ItemId { get; set; }
    [FirestoreProperty] public int Amount { get; set; }
}
