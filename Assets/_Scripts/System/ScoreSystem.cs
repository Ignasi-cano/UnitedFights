using UnityEngine;

public class ScoreSystem : Singleton<ScoreSystem>
{
    public int CurrentScore { get; private set; }
    public event System.Action<int> OnScoreChanged;
   
    [SerializeField] private int pointsPerEnemyKill = 100;
    [SerializeField] private int pointsPerCardPlayed = 10;
    [SerializeField] private int bonusForNoHeroDamage = 500;

    public int MaxDamageDealt { get; private set; }
    public int TotalGoldSpent { get; private set; }

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
        int totalDamageInAction = action.Amount; // The base damage of the action
        
        // Track max damage dealt to ENEMIES
        bool hittingEnemy = false;
        foreach(var target in action.Targets)
        {
            if (target is EnemyView) hittingEnemy = true;
            if (target is HeroView) heroTookDamage = true;
        }

        if (hittingEnemy && totalDamageInAction > MaxDamageDealt)
        {
            MaxDamageDealt = totalDamageInAction;
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
        MaxDamageDealt = 0;
        TotalGoldSpent = 0;
        heroTookDamage = false;
    }

    public void AddGoldSpent(int amount)
    {
        TotalGoldSpent += amount;
    }

    public void SaveFinalScore()
    {
        int finalScore = CalculateFinalScore();
        if (ScoreManager.Instance != null && AuthManager.Instance.IsLoggedIn)
        {
            string userId = AuthManager.Instance.CurrentUser.UserId;
            
            // 1. Save HighScore and update Elo
            ScoreManager.Instance.SaveScore(finalScore);
            
            // 2. Save Match History
            MatchRecord record = new MatchRecord
            {
                WinnerId = userId, // In this version, we assume player won if they reach here
                LoserId = "CPU",
                DamageDealt = finalScore // Using score as damage metric for now
            };
            ScoreManager.Instance.AddMatchRecord(record);
            
            // 3. Update Hero Stats (assuming we can get the active hero)
            if (GameManager.Instance != null && GameManager.Instance.ActiveHeroes.Count > 0)
            {
                string heroName = GameManager.Instance.ActiveHeroes[0].name;
                ScoreManager.Instance.UpdateHeroStats(userId, heroName, true);
            }
            
            Debug.Log($"[ScoreSystem] Final score {finalScore} and match data saved to Firebase.");
        }
        else
        {
            Debug.LogWarning("[ScoreSystem] Cannot save score: ScoreManager missing or user not logged in.");
        }
    }
}
