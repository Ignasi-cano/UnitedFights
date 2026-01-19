using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;
    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetupArrow(startPosition);
    }
    public CombatantView EndTargeting(Vector3 endPOsition)
    {
        arrowView.gameObject.SetActive(false);
        
        // ROBUST TARGETING: Use the camera ray from the mouse position
        // This is way more reliable than building a manual ray from a world position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayerMask))
        {
            Debug.Log($"[ManualTargetSystem] Success! Hit: {hit.collider.name} on Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}");
            
            if (hit.transform.GetComponentInParent<CombatantView>() is CombatantView combatantView)
            {
                return combatantView;
            }
            else
            {
                Debug.LogWarning($"[ManualTargetSystem] Hit {hit.collider.name}, but no CombatantView found in parent!");
            }
        }
        else
        {
            // DEBUG: Let's try to hit ANYTHING without the mask to tell the user if it's a mask issue
            if (Physics.Raycast(ray, out RaycastHit debugHit, 100f))
            {
                Debug.LogError($"[ManualTargetSystem] MISSED! You hit '{debugHit.collider.name}' (Layer: {LayerMask.LayerToName(debugHit.collider.gameObject.layer)}), BUT the TargetLayerMask blocked it! -> Add this layer to your TargetLayerMask in the Inspector.");
            }
            else
            {
                Debug.Log("[ManualTargetSystem] Raycast missed EVERYTHING. Ensure your hero has a BoxCollider and it's visible to the camera.");
            }
            Debug.DrawRay(ray.origin, ray.direction * 50f, Color.red, 3f);
        }
        return null;
    }
}
