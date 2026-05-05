using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public void AddEnemy(EnemyData enemyData)
    {
        if (slots == null || slots.Count == 0)
        {
            Debug.LogWarning("[EnemyBoardView] 'slots' list is empty! Auto-populating from children...");
            slots = new List<Transform>();
            foreach (Transform child in transform) {
                slots.Add(child);
            }
        }
        if (EnemyViews.Count >= slots.Count)
        {
            Debug.LogWarning($"[EnemyBoardView] Not enough slots to add enemy {enemyData.name}. Max slots: {slots.Count}");
            return;
        }

        Transform slot = slots[EnemyViews.Count];
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        enemyView.transform.parent = slot;
        EnemyViews.Add(enemyView);
    }
    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        if (enemyView == null) yield break;
        
        EnemyViews.Remove(enemyView);
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemyView.gameObject);
    }
}
