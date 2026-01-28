using UnityEngine;

public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
    [SerializeField] private EnemyView enemyViewPrefab;
    [SerializeField] private float enemyScaleFactor = 1.3f;

    public EnemyView CreateEnemyView(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        EnemyView enemyView = Instantiate(enemyViewPrefab, position, rotation);
        enemyView.transform.localScale = Vector3.one * enemyScaleFactor;
        enemyView.Setup(enemyData);
        return enemyView;
    }
}
