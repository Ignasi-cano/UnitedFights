using UnityEngine;
using TMPro;

public class GameScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.OnScoreChanged += UpdateScoreText;
            UpdateScoreText(ScoreSystem.Instance.CurrentScore);
        }
    }

    private void UpdateScoreText(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }

    private void OnDestroy()
    {
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.OnScoreChanged -= UpdateScoreText;
        }
    }
}
