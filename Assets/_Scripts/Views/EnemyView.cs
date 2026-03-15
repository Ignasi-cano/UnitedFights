using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    public int AttackPower { get; set; }
    public int PatternIndex { get; set; } = 0;
    public EnemyData EnemyData { get; private set; }
    public void Setup(EnemyData enemyData)
    {
        EnemyData = enemyData;
        AttackPower = enemyData.AttackPower;
        UpdateAttackText();
        SetupBase(enemyData.Health, enemyData.Image );
    }
    private void UpdateAttackText()
    {
        attackText.text = $"<color=#FFD700>Atk:</color>{AttackPower}";
    }
}
