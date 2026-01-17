using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    
    public MapNode Node { get; private set; }

    public void Setup(MapNode node)
    {
        Node = node;
        gameObject.name = $"Node_{node.ID}";
        button.onClick.AddListener(OnNodeClicked);
        
        // icon.sprite = GetSpriteForType(node.NodeType); // Needs icons later
    }

    private void OnNodeClicked()
    {
        MapSystem.Instance.SelectNode(Node.ID);
    }
}
