using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [Header("Intent UI")]
    [SerializeField] private EnemyIntentUI intentUI;

    [Header("Intent Positioning")]
    [SerializeField] private Vector3 intentLocalPosition = new Vector3(0f, 1.4f, 0f);
    [SerializeField] private bool forceIntentPositionOnSetup = true;

    [Header("Legacy UI (optional)")]
    [SerializeField] private TMP_Text legacyAttackText;

    public int AttackPower { get; private set; }
    public int PatternIndex { get; set; } = 0;
    public EnemyData EnemyData { get; private set; }

    public void Setup(EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Debug.LogError("[EnemyView] Setup called with null EnemyData.");
            return;
        }

        HideLegacyAttackText();

        EnemyData = enemyData;
        AttackPower = enemyData.AttackPower;
        PatternIndex = 0;

        SetupBase(enemyData.Health, enemyData.Image);

        PositionIntentUI();
        UpdateIntent();
    }

    public void UpdateIntent()
    {
        if (EnemyData == null)
        {
            HideIntent();
            return;
        }

        CardData nextCard = GetNextPatternCard();

        if (nextCard != null)
        {
            int displayedDamage = CalculateDisplayedIntentDamage(nextCard.IntentValue);
            ShowIntent(nextCard.IntentSprite, displayedDamage);
        }
        else
        {
            HideIntent();
        }
    }

    private CardData GetNextPatternCard()
    {
        if (EnemyData.AttackPattern == null || EnemyData.AttackPattern.Count == 0)
            return null;

        return EnemyData.AttackPattern[PatternIndex % EnemyData.AttackPattern.Count];
    }

    private int CalculateDisplayedIntentDamage(int baseDamage)
    {
        int finalDamage = baseDamage;

        if (GetStatusEffectStacks(StatusEffectType.BURN) > 0)
        {
            finalDamage = Mathf.FloorToInt(finalDamage * 0.5f);
        }

        return Mathf.Max(0, finalDamage);
    }

    private void ShowIntent(Sprite sprite, int value)
    {
        if (intentUI != null)
        {
            intentUI.Set(sprite, value);
        }
    }

    private void HideIntent()
    {
        if (intentUI != null)
        {
            intentUI.Hide();
        }
    }

    private void PositionIntentUI()
    {
        if (!forceIntentPositionOnSetup || intentUI == null) return;

        Transform intentTransform = intentUI.transform;

        RectTransform rect = intentTransform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 120f);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }
        else
        {
            intentTransform.localPosition = intentLocalPosition;
            intentTransform.localRotation = Quaternion.identity;
            intentTransform.localScale = Vector3.one;
        }
    }

    private void HideLegacyAttackText()
    {
        if (legacyAttackText != null)
        {
            legacyAttackText.gameObject.SetActive(false);
        }

        Transform legacy = transform.Find("AttackText");
        if (legacy != null)
        {
            legacy.gameObject.SetActive(false);
        }
    }
}