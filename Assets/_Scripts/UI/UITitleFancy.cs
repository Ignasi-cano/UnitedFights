using UnityEngine;
using DG.Tweening;

public class UITitleFancy : MonoBehaviour
{
    [SerializeField] private float floatAmount = 15f;
    [SerializeField] private float floatDuration = 2f;

    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.localPosition;
        StartFloating();
    }

    private void StartFloating()
    {
        transform.DOLocalMoveY(originalPosition.y + floatAmount, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
}
