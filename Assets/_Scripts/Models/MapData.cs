using System;
using System.Collections.Generic;

[Serializable]
public class MapData
{
    public List<MapNode> Nodes = new();
    public string CurrentNodeID;
}
