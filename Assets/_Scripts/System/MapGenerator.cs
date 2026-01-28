using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    public static MapData GenerateMap(int layers, int nodesPerLayer)
    {
        MapData mapData = new MapData();
        List<List<MapNode>> nodesByLayer = new();

        // 1. Generate Nodes
        for (int l = 0; l < layers; l++)
        {
            List<MapNode> layerNodes = new();
            int count = (l == 0 || l == layers - 1) ? 1 : Random.Range(2, nodesPerLayer + 1);

            for (int i = 0; i < count; i++)
            {
                MapNodeType type = GetRandomNodeType(l, layers);
                MapNode node = new MapNode($"{l}_{i}", type, new Vector2Int(l, i));
                layerNodes.Add(node);
                mapData.Nodes.Add(node);
            }
            nodesByLayer.Add(layerNodes);
        }

        // 2. Connect Layers
        for (int l = 0; l < layers - 1; l++)
        {
            List<MapNode> currentLayer = nodesByLayer[l];
            List<MapNode> nextLayer = nodesByLayer[l + 1];

            foreach (var node in currentLayer)
            {
                // Each node connects to at least 1 next node
                int connections = Random.Range(1, Mathf.Min(3, nextLayer.Count + 1));
                HashSet<int> indices = new();
                while (indices.Count < connections)
                {
                    indices.Add(Random.Range(0, nextLayer.Count));
                }

                foreach (int idx in indices)
                {
                    node.OutgoingConnections.Add(nextLayer[idx].ID);
                }
            }

            // Ensure every node in next layer has at least one incoming connection
            foreach (var nodeNext in nextLayer)
            {
                bool hasIncoming = false;
                foreach (var nodeCurr in currentLayer)
                {
                    if (nodeCurr.OutgoingConnections.Contains(nodeNext.ID))
                    {
                        hasIncoming = true;
                        break;
                    }
                }

                if (!hasIncoming)
                {
                    currentLayer[Random.Range(0, currentLayer.Count)].OutgoingConnections.Add(nodeNext.ID);
                }
            }
        }

        return mapData;
    }

    private static MapNodeType GetRandomNodeType(int layer, int totalLayers)
    {
        if (layer == 0) return MapNodeType.COMBAT;
        if (layer == totalLayers - 1) return MapNodeType.BOSS;
        
        // NEW: Force AUGMENT on the third row (layer 2)
        if (layer == 2) return MapNodeType.AUGMENT;

        float r = Random.value;
        if (r < 0.5f) return MapNodeType.COMBAT;
        if (r < 0.65f) return MapNodeType.ELITE;
        if (r < 0.75f) return MapNodeType.REST;
        if (r < 0.95f) return MapNodeType.SHOP;
        return MapNodeType.TREASURE;
    }
}
