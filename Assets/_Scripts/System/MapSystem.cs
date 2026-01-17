using UnityEngine;

public class MapSystem : Singleton<MapSystem>
{
    public MapData CurrentMap { get; private set; }

    public void GenerateNewMap()
    {
        CurrentMap = MapGenerator.GenerateMap(15, 4);
        Debug.Log("New map generated with " + CurrentMap.Nodes.Count + " nodes.");
    }

    public void SelectNode(string nodeID)
    {
        CurrentMap.CurrentNodeID = nodeID;
        // Logic to load scene based on node type
        var node = CurrentMap.Nodes.Find(n => n.ID == nodeID);
        Debug.Log($"Moving to node: {nodeID} of type {node.NodeType}");
    }
}
