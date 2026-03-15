using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    [SerializeField] private StatusEffectUI intentUI;
    public int AttackPower { get; set; }
    public int PatternIndex { get; set; } = 0;
    public EnemyData EnemyData { get; private set; }
    public void Setup(EnemyData enemyData)
    {
        EnemyData = enemyData;
        AttackPower = enemyData.AttackPower;
        UpdateIntent();
        SetupBase(enemyData.Health, enemyData.Image );
    }
    public void UpdateIntent()
    {
        if (EnemyData != null && EnemyData.AttackPattern != null && EnemyData.AttackPattern.Count > 0)
        {
            CardData nextCard = EnemyData.AttackPattern[PatternIndex % EnemyData.AttackPattern.Count];
            if (intentUI != null && nextCard.IntentSprite != null)
            {
                intentUI.gameObject.SetActive(true);
                intentUI.Set(nextCard.IntentSprite, nextCard.IntentValue);
                if (attackText != null) attackText.gameObject.SetActive(false);
            }
            else
            {
                if (intentUI != null) intentUI.gameObject.SetActive(false);
                if (attackText != null)
                {
                    attackText.gameObject.SetActive(true);
                    attackText.text = $"<color=#FFD700>Atk:</color>{nextCard.IntentValue}";
                }
            }
        }
        else
        {
            if (intentUI != null) intentUI.gameObject.SetActive(false);
            if (attackText != null)
            {
                attackText.gameObject.SetActive(true);
                attackText.text = $"<color=#FFD700>Atk:</color>{AttackPower}";
            }
        }
    }
}
