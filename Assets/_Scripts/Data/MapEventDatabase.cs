using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapEventDatabase", menuName = "Data/Map/Event Database")]
public class MapEventDatabase : ScriptableObject
{
    public List<MapEventData> AllEvents = new();

    public MapEventData GetRandomEvent()
    {
        if (AllEvents == null || AllEvents.Count == 0) return null;
        return AllEvents[Random.Range(0, AllEvents.Count)];
    }
}
