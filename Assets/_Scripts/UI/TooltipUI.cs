using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TooltipUI : PersistentSingleton<TooltipUI>
{
    [Header("References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TMP_Text contentText;
    
    [Header("Settings")]
    [SerializeField] private Vector2 offset = new Vector2(10, 10);
    
    private Canvas cachedCanvas;
    private RectTransform rectTransform;

    protected override void Awake()
    {
        base.Awake();
        rectTransform = GetComponent<RectTransform>();
        
        // Final safety: Ensure children are not blocking raycasts
        if (tooltipPanel != null)
        {
            var images = tooltipPanel.GetComponentsInChildren<Image>();
            foreach (var img in images) img.raycastTarget = false;
        }
        
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            UpdatePosition();
        }
    }

    public void Show(string content)
    {
        if (tooltipPanel == null || contentText == null) return;

        Debug.Log($"[TooltipUI] Displaying: {content}");
        gameObject.SetActive(true);
        tooltipPanel.SetActive(true);
        contentText.text = content;
        
        // Reset scale and alpha in case of inspector edits
        tooltipPanel.transform.localScale = Vector3.one;
        if (contentText.TryGetComponent<CanvasGroup>(out var g)) g.alpha = 1f;
        contentText.alpha = 1f;

        UpdatePosition();
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void UpdatePosition()
    {
        // Find/Refresh Canvas
        if (cachedCanvas == null || !cachedCanvas.isActiveAndEnabled)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in canvases)
            {
                if (c.isRootCanvas && c.isActiveAndEnabled) { cachedCanvas = c; break; }
            }
            if (cachedCanvas == null) cachedCanvas = Object.FindAnyObjectByType<Canvas>();

            if (cachedCanvas != null && transform.parent != cachedCanvas.transform)
            {
                transform.SetParent(cachedCanvas.transform, false);
                ResetRectTransform();
            }
        }

        if (cachedCanvas == null) return;

        // Positioning logic
        Vector2 mousePos = Input.mousePosition;
        
        if (cachedCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // In Overlay, world position = screen position
            transform.position = mousePos + offset;
        }
        else
        {
            // In Camera mode, convert to local point
            Camera cam = cachedCanvas.worldCamera != null ? cachedCanvas.worldCamera : Camera.main;
            RectTransform canvasRect = (RectTransform)cachedCanvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, mousePos, cam, out Vector2 localPoint);
            rectTransform.anchoredPosition = localPoint + offset;
        }

        // Pivot logic: flip tooltip if too close to screen edges
        float pivotX = (mousePos.x > Screen.width * 0.5f) ? 1.1f : -0.1f;
        float pivotY = (mousePos.y > Screen.height * 0.5f) ? 1.1f : -0.1f;
        rectTransform.pivot = new Vector2(pivotX, pivotY);

        // Debug log (throttled)
        if (Time.frameCount % 100 == 0)
        {
             Debug.Log($"[TooltipUI] Status: Active={tooltipPanel.activeInHierarchy}, Pos={transform.position}, Canvas={cachedCanvas.name}");
        }
    }

    private void ResetRectTransform()
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        Debug.Log("[TooltipUI] RectTransform reset for new Canvas.");
    }
}
