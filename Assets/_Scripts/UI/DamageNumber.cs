using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        // Auto-fix if reference is missing
        if (text == null) text = GetComponent<TMP_Text>();
        if (text == null) text = GetComponentInChildren<TMP_Text>();
    }

    public void Setup(string message, Color color)
    {
        if (text == null)
        {
            Debug.LogError($"[DamageNumber] No TMP_Text component found on {gameObject.name}!");
            Destroy(gameObject);
            return;
        }

        text.text = message;
        text.color = color;
        text.alpha = 1f; 

        transform.localScale = Vector3.zero;
        
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1;

        Sequence seq = DOTween.Sequence().SetLink(gameObject);
        seq.Append(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        seq.Append(transform.DOScale(1f, 0.1f));
        
        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            seq.Join(rect.DOAnchorPosY(rect.anchoredPosition.y + 100f, 0.8f).SetEase(Ease.OutSine));
        }

        seq.Insert(0.4f, text.DOFade(0, 0.4f));

        seq.OnComplete(() => {
            if (this != null && gameObject != null) Destroy(gameObject);
        });
    }
}
