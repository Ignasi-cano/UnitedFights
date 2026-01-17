using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapNode
{
    public string ID;
    public MapNodeType NodeType;
    public Vector2Int Position; // (Layer, IndexInLayer)
    public List<string> OutgoingConnections = new();

    public MapNode(string id, MapNodeType type, Vector2Int position)
    {
        ID = id;
        NodeType = type;
        Position = position;
    }
}
