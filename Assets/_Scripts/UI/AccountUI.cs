using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AccountUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text eloText;
    [SerializeField] private TMP_Text gamesPlayedText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private LeaderboardUI leaderboard;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ReturnToMenu);
        }
    }

    public void ToggleLeaderboard()
    {
        // Use the panel if assigned, otherwise fallback to the leaderboard object
        GameObject target = leaderboardPanel != null ? leaderboardPanel : (leaderboard != null ? leaderboard.gameObject : null);

        if (target != null)
        {
            bool isVisible = !target.activeSelf;
            target.SetActive(isVisible);
            Debug.Log($"[AccountUI] Toggling leaderboard. New state: {isVisible}");
            
            if (isVisible && leaderboard != null)
            {
                leaderboard.RefreshLeaderboard();
            }
        }
        else
        {
            Debug.LogWarning("[AccountUI] No leaderboard or panel assigned to toggle!");
        }
    }

    public void CloseLeaderboard()
    {
        GameObject target = leaderboardPanel != null ? leaderboardPanel : (leaderboard != null ? leaderboard.gameObject : null);
        if (target != null)
        {
            target.SetActive(false);
            Debug.Log("[AccountUI] Leaderboard closed.");
        }
    }

    private void ReturnToMenu()
    {
        MainMenu menu = FindFirstObjectByType<MainMenu>();
        if (menu != null)
        {
            menu.OpenMainMenuPanel();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
        if (leaderboard != null) leaderboard.RefreshLeaderboard();
    }

    public void RefreshUI()
    {
        if (AuthManager.Instance != null && AuthManager.Instance.IsLoggedIn && ScoreManager.Instance != null)
        {
            string userId = AuthManager.Instance.CurrentUser.UserId;
            ScoreManager.Instance.GetUserProfile(userId, (profile) => {
                if (emailText != null) emailText.text = $"Email: {profile.Email}";
                if (rankText != null) rankText.text = $"Rango: {profile.Tier}";
                if (eloText != null) eloText.text = $"Elo: {profile.Elo}";
                if (gamesPlayedText != null) gamesPlayedText.text = $"Partidas: {profile.GamesPlayed}";
                if (highScoreText != null) highScoreText.text = $"Récord: {profile.HighScore}";
            });
        }
    }
}
