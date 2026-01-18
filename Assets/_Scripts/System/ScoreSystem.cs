using UnityEngine;

public class ScoreSystem : Singleton<ScoreSystem>
{
    public int CurrentScore { get; private set; }
    public event System.Action<int> OnScoreChanged;
   
    [SerializeField] private int pointsPerEnemyKill = 100;
    [SerializeField] private int pointsPerCardPlayed = 10;
    [SerializeField] private int bonusForNoHeroDamage = 500;

    private bool heroTookDamage = false;

    private void Start()
    {
        // Subscribe to game actions
        ActionSystem.SubscribeReaction<KillEnemyGA>(OnEnemyKilled, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<PlayCardGA>(OnCardPlayed, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageDealt, ReactionTiming.POST);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(OnEnemyKilled, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(OnCardPlayed, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageDealt, ReactionTiming.POST);
    }

    private void OnEnemyKilled(KillEnemyGA action)
    {
        AddScore(pointsPerEnemyKill);
    }

    private void OnCardPlayed(PlayCardGA action)
    {
        AddScore(pointsPerCardPlayed);
    }

    private void OnDamageDealt(DealDamageGA action)
    {
        foreach(var target in action.Targets)
        {
            // Assuming HeroView is the class name for the player character
            if (target is HeroView)
            {
                heroTookDamage = true;
            }
        }
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
        
        // Reward gold based on score (example: 1 gold per 10 score)
        if (CurrencySystem.HasInstance)
        {
            CurrencySystem.Instance.AddGold(amount / 10);
        }
    }

    public int CalculateFinalScore()
    {
        int finalScore = CurrentScore;
       
        if (!heroTookDamage)
            finalScore += bonusForNoHeroDamage;
           
        return finalScore;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        heroTookDamage = false;
    }

    public void SaveFinalScore()
    {
        int finalScore = CalculateFinalScore();
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveScore(finalScore);
        }
        else
        {
            Debug.LogError("ScoreManager instance is missing, cannot save score!");
        }
    }
}
