using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

        // AUTO-FIX: Ensure Layout Group Exists
        // AUTO-FIX: Ensure Layout Group Exists and is Configured correctly
        VerticalLayoutGroup layout = contentContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        // Force settings even if it already existed
        layout.childForceExpandHeight = false;
        layout.childControlHeight = false; // RESPECT PREFAB HEIGHT
        layout.childControlWidth = false; // RESPECT PREFAB WIDTH - Fixes "text separated" and horizontal scroll issues
        layout.childForceExpandWidth = false;
        layout.spacing = 50; // Increased spacing as requested
        layout.childAlignment = TextAnchor.UpperCenter;

        // AUTO-FIX: Ensure Content Size Fitter Exists for Scrolling
        ContentSizeFitter csf = contentContainer.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        // Create new items
        for (int i = 0; i < scores.Count; i++)
        {
            var score = scores[i];
            GameObject entry = Instantiate(scoreEntryPrefab, contentContainer);
            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            
            string position = (i + 1).ToString();
            
            if (texts.Length >= 2)
            {
                // Slot 0: #Pos - Email [Tier]
                texts[0].text = $"#{position} - {score.Email} [{score.Tier}]"; 
                // Slot 1: Score | Elo
                texts[1].text = $"Score: {score.HighScore} | Elo: {score.Elo}";
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"#{position}. {score.Email} ({score.Tier}) - Puntos: {score.HighScore}";
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
