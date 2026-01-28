using UnityEngine;
using TMPro;
using DG.Tweening;

public class TurnNotificationUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TMP_Text turnText;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<HeroTurnStartGA>(OnHeroTurnStart, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.PRE);
        
        // Ensure hidden at start
        if (panel != null) panel.alpha = 0;
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<HeroTurnStartGA>(OnHeroTurnStart, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.PRE);
    }

    private void OnHeroTurnStart(HeroTurnStartGA action)
    {
        ShowNotification("YOUR TURN", new Color(0.2f, 0.8f, 0.2f)); // Greenish
    }

    private void OnEnemyTurnStart(EnemyTurnGA action)
    {
        ShowNotification("ENEMY TURN", new Color(1f, 0.2f, 0.2f)); // Reddish
    }

    private void ShowNotification(string text, Color color)
    {
        if (panel == null || turnText == null) return;

        turnText.text = text;
        turnText.color = color;

        // Sequence: Fade In & Scale Up -> Wait -> Fade Out & Scale Down
        panel.DOKill();
        panel.transform.DOKill();

        panel.alpha = 0;
        panel.transform.localScale = Vector3.one * 0.8f;

        Sequence seq = DOTween.Sequence();
        seq.Append(panel.DOFade(1, 0.3f));
        seq.Join(panel.transform.DOScale(1.1f, 0.3f).SetEase(Ease.OutBack));
        seq.AppendInterval(1f);
        seq.Append(panel.DOFade(0, 0.3f));
        seq.Join(panel.transform.DOScale(0.9f, 0.3f).SetEase(Ease.InSine));
    }
}
