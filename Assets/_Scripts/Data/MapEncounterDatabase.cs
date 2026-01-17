using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(menuName = "Data/MapEncounterDatabase")]
public class MapEncounterDatabase : ScriptableObject
{
    [Serializable]
    public class EncounterGroup
    {
        public MapNodeType NodeType;
        public List<EnemyData> Enemies;
    }

    [SerializeField] private List<EncounterGroup> encounters;

    public List<EnemyData> GetEnemiesForNode(MapNodeType type)
    {
        var group = encounters.Find(e => e.NodeType == type);
        return group?.Enemies ?? new List<EnemyData>();
    }
}
