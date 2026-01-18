using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject scoreEntryPrefab;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnLeaderboardLoaded += DisplayLeaderboard;
            RefreshLeaderboard();
        }
    }

    public void RefreshLeaderboard()
    {
        if (statusText != null) statusText.text = "Loading Leaderboard...";
        ScoreManager.Instance.LoadLeaderboard();
    }

    private void DisplayLeaderboard(List<PlayerScore> scores)
    {
        if (statusText != null) statusText.text = "";

        // Clear existing items
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Create new items
        foreach (var score in scores)
        {
            GameObject entry = Instantiate(scoreEntryPrefab, contentContainer);
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            
            if (texts.Length >= 2)
            {
                // Slot 0: Name + Tier
                texts[0].text = $"{score.Email} [{score.Tier}]"; 
                // Slot 1: Score + Elo
                texts[1].text = $"Score: {score.HighScore} | Elo: {score.Elo}";
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{score.Email} ({score.Tier}) - Puntos: {score.HighScore}";
            }
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.HasInstance)
        {
            ScoreManager.Instance.OnLeaderboardLoaded -= DisplayLeaderboard;
        }
    }
}
