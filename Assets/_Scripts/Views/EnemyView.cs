using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [Header("Intent Prefab")]
    [SerializeField] private EnemyIntentUI intentUIPrefab;
    [SerializeField] private Vector3 intentOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Target Lines")]
    [SerializeField] private EnemyTargetLine targetLinePrefab;
    [SerializeField] private Vector3 lineStartOffset = new Vector3(0f, 0.4f, 0f);
    [SerializeField] private bool showTargetLines = true;

    [Header("Legacy UI (optional)")]
    [SerializeField] private TMP_Text legacyAttackText;

    private EnemyIntentUI spawnedIntentUI;
    private readonly List<EnemyTargetLine> spawnedTargetLines = new();
    private readonly List<CombatantView> currentIntentTargets = new();

    public int AttackPower { get; private set; }
    public int PatternIndex { get; set; } = 0;
    public EnemyData EnemyData { get; private set; }

    public IReadOnlyList<CombatantView> CurrentIntentTargets => currentIntentTargets;

    public CombatantView CurrentManualIntentTarget
    {
        get
        {
            if (currentIntentTargets.Count == 0) return null;
            return currentIntentTargets[0];
        }
    }

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

        CreateIntentUI();
        UpdateIntent();
    }

    private void CreateIntentUI()
    {
        if (intentUIPrefab == null)
        {
            Debug.LogWarning("[EnemyView] Intent UI Prefab is not assigned.");
            return;
        }

        if (spawnedIntentUI != null) return;

        spawnedIntentUI = Instantiate(
            intentUIPrefab,
            transform.position + intentOffset,
            Quaternion.identity,
            transform
        );

        spawnedIntentUI.transform.localPosition = intentOffset;
    }

    private void LateUpdate()
    {
        if (spawnedIntentUI != null)
        {
            spawnedIntentUI.transform.localPosition = intentOffset;
        }
    }

    public void UpdateIntent()
    {
        if (EnemyData == null)
        {
            HideIntent();
            ClearTargetLines();
            return;
        }

        CardData nextCard = GetNextPatternCard();

        if (nextCard != null)
        {
            int displayedDamage = CalculateDisplayedIntentDamage(nextCard.IntentValue);
            ShowIntent(nextCard.IntentSprite, displayedDamage);
            RefreshIntentTargetsAndLines(nextCard);
        }
        else
        {
            HideIntent();
            ClearTargetLines();
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
        if (spawnedIntentUI != null)
        {
            spawnedIntentUI.Set(sprite, value);
        }
    }

    private void HideIntent()
    {
        if (spawnedIntentUI != null)
        {
            spawnedIntentUI.Hide();
        }
    }

    private void RefreshIntentTargetsAndLines(CardData card)
    {
        currentIntentTargets.Clear();

        if (card == null)
        {
            ClearTargetLines();
            return;
        }

        if (card.ManualTargetEffect != null)
        {
            CombatantView target = HeroSystem.Instance.GetRandomFrontlineHero();
            AddIntentTarget(target);
        }

        if (card.OtherEffects != null)
        {
            foreach (var effectWrapper in card.OtherEffects)
            {
                if (effectWrapper == null || effectWrapper.TargetMode == null) continue;

                List<CombatantView> targets = effectWrapper.TargetMode.GetTargets();
                if (targets == null) continue;

                foreach (CombatantView target in targets)
                {
                    AddIntentTarget(target);
                }
            }
        }

        RebuildTargetLines();
    }

    private void AddIntentTarget(CombatantView target)
    {
        if (target == null) return;
        if (target.CurrentHealth <= 0 || target.IsDying) return;
        if (currentIntentTargets.Contains(target)) return;

        currentIntentTargets.Add(target);
    }

    private void RebuildTargetLines()
    {
        ClearTargetLines();

        if (!showTargetLines || targetLinePrefab == null) return;

        foreach (CombatantView target in currentIntentTargets)
        {
            EnemyTargetLine line = Instantiate(
                targetLinePrefab,
                transform.position + lineStartOffset,
                Quaternion.identity
            );

            line.Setup(transform, target.transform);
            spawnedTargetLines.Add(line);
        }
    }

    public void ClearTargetLines()
    {
        for (int i = spawnedTargetLines.Count - 1; i >= 0; i--)
        {
            if (spawnedTargetLines[i] != null)
            {
                Destroy(spawnedTargetLines[i].gameObject);
            }
        }

        spawnedTargetLines.Clear();
    }

    private void HideLegacyAttackText()
    {
        if (legacyAttackText != null)
            legacyAttackText.gameObject.SetActive(false);

        Transform legacy = transform.Find("AttackText");
        if (legacy != null)
            legacy.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        ClearTargetLines();

        if (spawnedIntentUI != null)
        {
            Destroy(spawnedIntentUI.gameObject);
        }
    }
}