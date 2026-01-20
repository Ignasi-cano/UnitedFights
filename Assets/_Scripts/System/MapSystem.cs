using UnityEngine;

public class MapSystem : PersistentSingleton<MapSystem>
{
    public MapData CurrentMap { get; private set; }
    public MapNode CurrentNode { get; private set; }
    public event System.Action OnMapUpdated;

    public void GenerateNewMap()
    {
        CurrentMap = MapGenerator.GenerateMap(15, 4);
        Debug.Log("New map generated with " + CurrentMap.Nodes.Count + " nodes.");
    }

    public void SelectNode(string nodeID)
    {
        if (CurrentMap == null)
        {
            Debug.LogError("[MapSystem] Cannot select node because CurrentMap is null!");
            return;
        }

        MapNode node = CurrentMap.Nodes.Find(n => n.ID == nodeID);
        if (node == null)
        {
            Debug.LogError($"[MapSystem] Could not find node with ID: {nodeID}");
            return;
        }

        CurrentMap.CurrentNodeID = nodeID;
        CurrentNode = node;
        
        Debug.Log($"[MapSystem] Successfully selected node: {nodeID} ({node.NodeType}). Loading scene...");

        // Logic to load scene based on node type
        if (CurrentNode.NodeType == MapNodeType.COMBAT || 
            CurrentNode.NodeType == MapNodeType.ELITE || 
            CurrentNode.NodeType == MapNodeType.BOSS)
        {
            Debug.Log("[MapSystem] Loading 'unitedfights' for combat encounter.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("unitedfights");
        }
        else if (CurrentNode.NodeType == MapNodeType.SHOP)
        {
            Debug.Log("[MapSystem] Entering Shop.");
            if (ShopView.Instance != null)
            {
                ShopView.Instance.OpenShop();
            }
            else
            {
                Debug.LogError("[MapSystem] ShopView instance not found in scene!");
            }
        }
        else if (CurrentNode.NodeType == MapNodeType.REST)
        {
            Debug.Log("[MapSystem] Entering Rest Node.");
            if (RestView.Instance != null)
            {
                RestView.Instance.OpenRest();
            }
            else
            {
                Debug.LogError("[MapSystem] RestView instance not found in scene!");
            }
        }
        else
        {
            Debug.LogWarning($"[MapSystem] Node type {CurrentNode.NodeType} has no specific scene logic implemented yet! Staying on map.");
        }
    }

    public void RefreshMap()
    {
        OnMapUpdated?.Invoke();
    }

    public void ResetMap()
    {
        CurrentMap = null;
        CurrentNode = null;
        Debug.Log("[MapSystem] Map state reset.");
    }
}
