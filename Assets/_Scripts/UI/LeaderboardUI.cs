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

        // FIX: Force Content Centering and Stretching
        RectTransform rect = contentContainer as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
        }

        // AUTO-FIX: Ensure Layout Group Exists and is Configured correctly
        VerticalLayoutGroup layout = contentContainer.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = contentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        
        // Settings for proper separation and positioning
        layout.childControlHeight = false; // WE WILL SET HEIGHT MANUALLY
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true; // FORCE FILL WIDTH
        layout.childForceExpandWidth = true; // FILL WIDTH
        layout.spacing = 10; // Reduced spacing to bring items closer
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.childAlignment = TextAnchor.UpperCenter;

        // AUTO-FIX: Ensure Content Size Fitter Exists for Scrolling
        ContentSizeFitter csf = contentContainer.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = contentContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // NEW: Disable Horizontal Scrolling on the ScrollRect
        ScrollRect scrollRect = contentContainer.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

        // Create new items
        for (int i = 0; i < scores.Count; i++)
        {
            var score = scores[i];
            GameObject entry = Instantiate(scoreEntryPrefab, contentContainer);
            
            // NEW: Force the entry height to be bigger than before (+50)
            RectTransform entryRect = entry.GetComponent<RectTransform>();
            if (entryRect != null)
            {
                entryRect.sizeDelta = new Vector2(entryRect.sizeDelta.x, 140f); // Reduced height
            }

            TextMeshProUGUI[] texts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            
            string position = (i + 1).ToString();
            string tierColor = GetTierColor(score.Tier);
            
            if (texts.Length >= 2)
            {
                // Slot 0: #Pos - Email [Tier]
                texts[0].text = $"#{position} - {score.Email} <color={tierColor}>[{score.Tier}]</color>"; 
                // Slot 1: Score | Elo
                texts[1].text = $"Score: {score.HighScore} | Elo: {score.Elo}";
            }
            else if (texts.Length == 1)
            {
                texts[0].text = $"#{position}. {score.Email} <color={tierColor}>({score.Tier})</color> - Puntos: {score.HighScore}";
            }
        }
    }

    private string GetTierColor(string tier)
    {
        switch (tier?.ToLower())
        {
            case "platinum": return "#00FFFF"; // Cyan
            case "gold": return "#FFD700"; // Gold
            case "silver": return "#C0C0C0"; // Silver
            case "bronze": return "#CD7F32"; // Bronze
            default: return "#FFFFFF"; // White
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
