using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryUI : Singleton<VictoryUI>
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI goldSpentText;
    [SerializeField] private TextMeshProUGUI maxDamageText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private LeaderboardUI leaderboard;
    [SerializeField] private Button continueButton;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        if (SceneManager.GetActiveScene().name == "Victory")
        {
            Show();
        }
    }

    public void Show()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[VictoryUI] No panel assigned! Saving score and returning to map automatically.");
            if (ScoreSystem.Instance != null) ScoreSystem.Instance.SaveFinalScore();
            SceneManager.LoadScene("MapScene");
            return;
        }
        
        if (ScoreSystem.Instance != null && ScoreManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            if (scoreText != null) scoreText.text = $"Puntos: {ScoreSystem.Instance.CurrentScore}";
            if (goldSpentText != null) goldSpentText.text = $"Oro Gastado: {ScoreSystem.Instance.TotalGoldSpent}";
            if (maxDamageText != null) maxDamageText.text = $"Daño Máximo: {ScoreSystem.Instance.MaxDamageDealt}";

            string userId = AuthManager.Instance.CurrentUser.UserId;
            ScoreManager.Instance.GetUserProfile(userId, (profile) => {
                if (rankText != null) rankText.text = $"Rango: {profile.Tier}";
                if (eloText != null) eloText.text = $"Elo: {profile.Elo}";
            });
            
            if (leaderboard != null) leaderboard.RefreshLeaderboard();
            
            // Save to Firebase when showing the victory screen
            ScoreSystem.Instance.SaveFinalScore();
        }
    }

    public void ToggleLeaderboard()
    {
        if (leaderboard != null)
        {
            leaderboard.gameObject.SetActive(!leaderboard.gameObject.activeSelf);
            if (leaderboard.gameObject.activeSelf)
            {
                leaderboard.RefreshLeaderboard();
            }
        }
    }

    private void OnContinuePressed()
    {
        SceneManager.LoadScene("MapScene");
    }
}
