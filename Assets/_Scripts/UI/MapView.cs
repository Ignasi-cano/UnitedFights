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

    private Dictionary<string, MapNodeView> nodeViews = new();

    private void Start()
    {
        if (MapSystem.Instance.CurrentMap == null)
        {
            MapSystem.Instance.GenerateNewMap();
        }
        RenderMap(MapSystem.Instance.CurrentMap);
    }

    public void RenderMap(MapData map)
    {
        // 1. Clear existing
        foreach (Transform child in nodesContainer) Destroy(child.gameObject);
        foreach (Transform child in linesContainer) Destroy(child.gameObject);
        nodeViews.Clear();

        // 2. Instantiate Nodes
        foreach (var node in map.Nodes)
        {
            MapNodeView view = Instantiate(nodePrefab, nodesContainer);
            view.Setup(node);
            
            // Calculate position
            float x = node.Position.x * layerSpacing;
            float y = (node.Position.y - 1.5f) * nodeSpacing; // Basic centering offset
            view.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            
            nodeViews.Add(node.ID, view);
        }

        // 3. Draw Connections
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

    private void DrawConnection(RectTransform start, RectTransform end)
    {
        Image line = Instantiate(linePrefab, linesContainer);
        RectTransform rect = line.GetComponent<RectTransform>();
        
        Vector2 dir = end.anchoredPosition - start.anchoredPosition;
        float distance = dir.magnitude;
        
        rect.anchoredPosition = start.anchoredPosition + dir / 2f;
        rect.sizeDelta = new Vector2(distance, 5f); // 5f is line thickness
        rect.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
    }
}
