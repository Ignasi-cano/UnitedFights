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
            
            // Assuming the prefab has a component or we find text components
            // For simplicity, finding TMP_Text components in children
            // Ideally, create a LeaderboardEntry script
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            
            if (texts.Length >= 2)
            {
                // Format: 1. Email: Score
                texts[0].text = score.Email; // Suggest masking email for privacy in real app
                texts[1].text = score.HighScore.ToString();
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"{score.Email} : {score.HighScore}";
            }
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnLeaderboardLoaded -= DisplayLeaderboard;
        }
    }
}
