using UnityEngine;

public class MapSystem : PersistentSingleton<MapSystem>
{
    public MapData CurrentMap { get; private set; }
    public MapNode CurrentNode { get; private set; }
    public event System.Action OnMapUpdated;
    public static event System.Action<MapNode> OnNodeSelected;

    protected override void Awake()
    {
        base.Awake();
        Debug.Log($"<color=cyan>[MapSystem Locator]</color> I am active on GameObject: <b>{gameObject.name}</b> in Scene: <b>{gameObject.scene.name}</b>", gameObject);
    }

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
        OnNodeSelected?.Invoke(node);
        
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
        else if (CurrentNode.NodeType == MapNodeType.AUGMENT)
        {
            Debug.Log("[MapSystem] Entering Augment Choice.");
            if (AugmentSelectionUI.Instance != null)
            {
                AugmentSelectionUI.Instance.Open();
            }
            else
            {
                Debug.LogError("[MapSystem] AugmentSelectionUI instance not found in scene!");
            }
        }
        else if (CurrentNode.NodeType == MapNodeType.TREASURE)
        {
            Debug.Log("[MapSystem] Entering Treasure Event.");
            bool uiExists = RandomEventUI.Instance != null;
            bool dbExists = eventDatabase != null;

            if (uiExists && dbExists)
            {
                var randomEvent = eventDatabase.GetRandomEvent();
                if (randomEvent != null)
                {
                    RandomEventUI.Instance.Open(randomEvent);
                }
                else
                {
                    Debug.LogWarning("[MapSystem] EventDatabase is EMPTY! No events found in the list.");
                    RefreshMap();
                }
            }
            else
            {
                if (!uiExists) Debug.LogError("[MapSystem] CRITICAL: RandomEventUI (Manager) not found in the scene!");
                if (!dbExists) Debug.LogError("[MapSystem] CRITICAL: EventDatabase is NOT ASSIGNED in the MapSystem inspector!");
                RefreshMap();
            }
        }
        else
        {
            Debug.LogWarning($"[MapSystem] Node type {CurrentNode.NodeType} has no specific scene logic implemented yet! Staying on map.");
        }
    }

    [Header("Event System")]
    [SerializeField] private MapEventDatabase eventDatabase;

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
