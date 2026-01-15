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
    }

    public void CreateUserDocument(string userId, string email)
    {
        if (db == null) return;

        Dictionary<string, object> userData = new()
        {
            { "email", email },
            { "highScore", 0 },
            { "gamesPlayed", 0 },
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

                Dictionary<string, object> updates = new()
                {
                    { "gamesPlayed", gamesPlayed + 1 },
                    { "lastScore", score },
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

                    scores.Add(new PlayerScore
                    {
                        Email = email,
                        HighScore = score,
                        GamesPlayed = games
                    });
                }

                OnLeaderboardLoaded?.Invoke(scores);
            });
    }
}

[Serializable]
public class PlayerScore
{
    public string Email;
    public int HighScore;
    public int GamesPlayed;
}
