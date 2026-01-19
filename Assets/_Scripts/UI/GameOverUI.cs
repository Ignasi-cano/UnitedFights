using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button returnButton;
    [SerializeField] private Button scoreButton;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private LeaderboardUI leaderboard;

    private void Start()
    {
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (scoreButton != null)
        {
            scoreButton.onClick.AddListener(ToggleLeaderboard);
        }
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void ToggleLeaderboard()
    {
        GameObject target = leaderboardPanel != null ? leaderboardPanel : (leaderboard != null ? leaderboard.gameObject : null);

        if (target != null)
        {
            bool isVisible = !target.activeSelf;
            target.SetActive(isVisible);

            if (isVisible && leaderboard != null)
            {
                leaderboard.RefreshLeaderboard();
            }
        }
        else
        {
            Debug.LogWarning("[GameOverUI] No leaderboard or panel assigned to toggle!");
        }
    }
}
