using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectionCircle;
    
    public MapNode Node { get; private set; }

    private void Awake()
    {
        // Explicitly find the button component (usually on the root or immediate child)
        if (button == null) button = GetComponent<Button>();
        if (button == null) button = GetComponentInChildren<Button>(true);
        
        // Find the icon Image (looking specifically for the Sprite child)
        if (icon == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject != this.gameObject && (button == null || img.gameObject != button.gameObject))
                {
                    icon = img;
                    break;
                }
            }
        }
    }

    public void Setup(MapNode node, Sprite nodeSprite = null)
    {
        Node = node;
        gameObject.name = $"Node_{node.ID}";
        
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnNodeClicked);
        }
        
        // Ensure all components are centered and reset
        RectTransform rt = GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchorMin = new Vector2(0, 0.5f); // Anchor to Left-Center
        rt.anchorMax = new Vector2(0, 0.5f); // Anchor to Left-Center
        
        if (icon != null)
        {
            if (nodeSprite != null) icon.sprite = nodeSprite;
            icon.raycastTarget = false; 
            
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0); 
            iconRt.anchorMax = new Vector2(1, 1); 
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.localScale = Vector3.one;
        }

        if (selectionCircle != null)
        {
            Image scImg = selectionCircle.GetComponent<Image>();
            if (scImg != null) scImg.raycastTarget = false;
        }

        // Final safety: ensure absolute NO other images block the click
        foreach (var img in GetComponentsInChildren<Image>(true))
        {
            // Only the button's own image should be a raycast target
            if (button != null && img.gameObject == button.gameObject) continue;
            img.raycastTarget = false;
        }

        if (button != null)
        {
            RectTransform btnRt = button.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0, 0);
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;

            // Hide any default "Button" text
            var texts = button.GetComponentsInChildren<Text>(true);
            foreach (var t in texts) t.enabled = false;
            
            // Make the button background invisible but still clickable
            Image btnImg = button.GetComponent<Image>();
            if (btnImg != null)
            {
                Color c = btnImg.color;
                c.a = 0f;
                btnImg.color = c;
            }

            // Handle TMPro if present (using reflection to avoid compile error if TMPro is missing)
            HideTMPro(button.gameObject);
        }
    }

    private void HideTMPro(GameObject go)
    {
        // Try to find TMPro components without adding a hard dependency
        foreach (var comp in go.GetComponentsInChildren<Component>(true))
        {
            if (comp.GetType().Name.Contains("TextMeshPro"))
            {
                var enabledProp = comp.GetType().GetProperty("enabled");
                if (enabledProp != null) enabledProp.SetValue(comp, false);
            }
        }
    }

    public void SetSelectable(bool selectable)
    {
        if (button != null) button.interactable = selectable;
        
        if (selectionCircle != null) selectionCircle.SetActive(selectable);

        if (icon != null)
        {
            // Dim the icon if it's not selectable
            Color c = icon.color;
            c.a = selectable ? 1f : 0.3f;
            icon.color = c;
        }
    }

    private void OnNodeClicked()
    {
        Debug.Log($"[UI] Node clicked: {Node.ID} of type {Node.NodeType}");
        MapSystem.Instance.SelectNode(Node.ID);
    }
}
