using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapView : MonoBehaviour
{
    [SerializeField] private MapNodeView nodePrefab;
    [SerializeField] private RectTransform nodesContainer;
    [SerializeField] private RectTransform linesContainer;
    [SerializeField] private Image linePrefab;
    
    [Header("Layout Settings")]
    [SerializeField] private float layerSpacing = 200f;
    [SerializeField] private float nodeSpacing = 150f;
    [SerializeField] private Vector2 nodeSize = new Vector2(100f, 100f);

    [Header("Visuals")]
    [SerializeField] private Sprite combatIcon;
    [SerializeField] private Sprite eliteIcon;
    [SerializeField] private Sprite restIcon;
    [SerializeField] private Sprite shopIcon;
    [SerializeField] private Sprite treasureIcon;
    [SerializeField] private Sprite bossIcon;
    [SerializeField] private Sprite augmentIcon;

    private Dictionary<string, MapNodeView> nodeViews = new();

    private void Start()
    {
        if (MapSystem.Instance.CurrentMap == null)
        {
            MapSystem.Instance.GenerateNewMap();
        }
        
        if (MapSystem.Instance.CurrentMap != null)
        {
            RenderMap(MapSystem.Instance.CurrentMap);
        }

        MapSystem.Instance.OnMapUpdated += RefreshNodeInteractivity;
    }

    private void OnDestroy()
    {
        if (MapSystem.HasInstance)
        {
            MapSystem.Instance.OnMapUpdated -= RefreshNodeInteractivity;
        }
    }

    // Updated MapView.cs logic for better scrolling and positioning
    public void RenderMap(MapData map)
    {
        Canvas.ForceUpdateCanvases(); // Ensure layout is calculated
        
        RectTransform mainRect = GetComponent<RectTransform>();
        RectTransform viewportRect = mainRect.parent as RectTransform;
        
        // 1. Setup Content Anchors (Horizontal Scrolling, Stretch Height)
        mainRect.anchorMin = new Vector2(0, 0); 
        mainRect.anchorMax = new Vector2(0, 1); 
        mainRect.pivot = new Vector2(0, 0.5f);  
        mainRect.anchoredPosition = Vector2.zero;
        mainRect.offsetMin = new Vector2(0, 0);
        mainRect.offsetMax = new Vector2(0, 0);

        // Ensure containers fill the Content and have correct scale
        ResetContainer(nodesContainer);
        ResetContainer(linesContainer);
        nodesContainer.SetAsLastSibling(); // Ensure nodes are drawn ON TOP of lines

        // 2. Clear existing
        foreach (Transform child in nodesContainer) Destroy(child.gameObject);
        foreach (Transform child in linesContainer) Destroy(child.gameObject);
        nodeViews.Clear();

        // Grouping logic...
        int maxLayer = 0;
        Dictionary<int, List<MapNode>> nodesByLayer = new();
        foreach (var node in map.Nodes)
        {
            if (!nodesByLayer.ContainsKey(node.Position.x))
                nodesByLayer[node.Position.x] = new List<MapNode>();
            nodesByLayer[node.Position.x].Add(node);
            if (node.Position.x > maxLayer) maxLayer = node.Position.x;
        }

        // 3. Set Size (Horizontal)
        float startPadding = layerSpacing / 2f; 
        float contentWidth = (maxLayer + 1) * layerSpacing + startPadding;
        
        // Safety: If viewport height is 0, use a reasonable fallback (e.g. 600)
        float contentHeight = viewportRect != null ? viewportRect.rect.height : 600f;
        if (contentHeight <= 0) contentHeight = 600f;
        
        mainRect.sizeDelta = new Vector2(contentWidth, 0); 

        // 4. Instantiate Nodes
        foreach (var layerKvp in nodesByLayer)
        {
            int layer = layerKvp.Key;
            var nodesInLayer = layerKvp.Value;
            
            for (int i = 0; i < nodesInLayer.Count; i++)
            {
                var node = nodesInLayer[i];
                MapNodeView view = Instantiate(nodePrefab, nodesContainer);
                view.Setup(node, GetSpriteForType(node.NodeType));

                // X = Layer * spacing + padding
                float x = startPadding + layer * layerSpacing;
                
                // Y = Centered around 0 
                // We use the nodesInLayer count to center them vertically in the viewport
                float layerHeight = (nodesInLayer.Count - 1) * nodeSpacing;
                float startY = layerHeight / 2f;
                float y = startY - i * nodeSpacing;

                RectTransform nodeRect = view.GetComponent<RectTransform>();
                nodeRect.localScale = Vector3.one;
                nodeRect.sizeDelta = nodeSize; // Force consistent size
                nodeRect.anchoredPosition = new Vector2(x, y);
                
                nodeViews.Add(node.ID, view);
            }
        }

        RefreshNodeInteractivity();

        // 5. Draw Connections
        foreach (var node in map.Nodes)
        {
            foreach (string connectionID in node.OutgoingConnections)
            {
                if (nodeViews.TryGetValue(node.ID, out var start) && 
                    nodeViews.TryGetValue(connectionID, out var end))
                {
                    DrawConnection(start.GetComponent<RectTransform>(), end.GetComponent<RectTransform>());
                }
            }
        }
    }

    private void ResetContainer(RectTransform container)
    {
        if (container == null) return;
        container.anchorMin = Vector2.zero;
        container.anchorMax = Vector2.one;
        container.offsetMin = Vector2.zero;
        container.offsetMax = Vector2.zero;
        container.localScale = Vector3.one;
    }

    private void DrawConnection(RectTransform start, RectTransform end)
    {
        Image line = Instantiate(linePrefab, linesContainer);
        RectTransform rect = line.GetComponent<RectTransform>();
        
        rect.localScale = Vector3.one;
        // Sync anchors with nodes (Left-Center) to ensure anchoredPosition math matches
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        
        Vector2 startPos = start.anchoredPosition;
        Vector2 endPos = end.anchoredPosition;
        
        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;
        
        line.raycastTarget = false; // Prevent lines from blocking node clicks
        rect.anchoredPosition = startPos;
        rect.sizeDelta = new Vector2(distance, 5f);
        rect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }

    private Sprite GetSpriteForType(MapNodeType type)
    {
        return type switch
        {
            MapNodeType.COMBAT => combatIcon,
            MapNodeType.ELITE => eliteIcon,
            MapNodeType.REST => restIcon,
            MapNodeType.SHOP => shopIcon,
            MapNodeType.TREASURE => treasureIcon,
            MapNodeType.BOSS => bossIcon,
            MapNodeType.AUGMENT => augmentIcon,
            _ => combatIcon
        };
    }
    public void RefreshNodeInteractivity()
    {
        if (MapSystem.Instance.CurrentMap == null) return;
        
        string currentID = MapSystem.Instance.CurrentMap.CurrentNodeID;
        MapNode currentNode = null;
        if (!string.IsNullOrEmpty(currentID))
        {
            currentNode = MapSystem.Instance.CurrentMap.Nodes.Find(n => n.ID == currentID);
        }

        foreach (var kvp in nodeViews)
        {
            MapNode node = kvp.Value.Node;
            bool isSelectable = false;

            if (string.IsNullOrEmpty(currentID))
            {
                // First layer is selectable if we haven't started
                isSelectable = node.Position.x == 0;
            }
            else if (currentNode != null)
            {
                // Reachable if it's an outgoing connection from our current node
                isSelectable = currentNode.OutgoingConnections.Contains(node.ID);
            }

            kvp.Value.SetSelectable(isSelectable);
        }
    }
}
