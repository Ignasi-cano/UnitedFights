using UnityEngine;

public class DamageNumbersSystem : Singleton<DamageNumbersSystem>
{
    [SerializeField] private GameObject damageNumberPrefab;

    public void Show(Vector3 worldPosition, string message, Color color)
    {
        if (damageNumberPrefab == null) return;

        // Buscamos el Canvas que tenga un CanvasScaler (el de la UI real)
        Canvas mainCanvas = null;
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var c in canvases)
        {
            if (c.GetComponent<UnityEngine.UI.CanvasScaler>() != null)
            {
                mainCanvas = c;
                break;
            }
        }

        // Fallback si no hay CanvasScaler
        if (mainCanvas == null) mainCanvas = Object.FindFirstObjectByType<Canvas>();

        if (mainCanvas == null)
        {
            Debug.LogWarning("[DamageNumbersSystem] No se encontró un Canvas válido!");
            return;
        }

        // Instanciamos dentro del Canvas
        GameObject go = Instantiate(damageNumberPrefab, mainCanvas.transform);
        go.name = "DmgNum_" + message; // Para que lo veas claro en la Jerarquía
        go.layer = 5; // Forzamos capa "UI"
        
        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            go.transform.SetAsLastSibling(); // Siempre al frente
            rect.localScale = Vector3.one; // ¡IMPORTANTE! Evitamos escala 0
            rect.localRotation = Quaternion.identity;
            
            // Calculamos posición exacta en el Canvas
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1.5f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mainCanvas.transform as RectTransform, 
                screenPoint, 
                mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main, 
                out Vector2 localPoint);
            
            rect.anchoredPosition = localPoint;
            rect.localPosition = new Vector3(localPoint.x, localPoint.y, 0); // Forzamos Z a cero
        }

        DamageNumber dn = go.GetComponent<DamageNumber>();
        if (dn != null) dn.Setup(message, color);
    }
}
