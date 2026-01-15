using Unity.IntegerTime;
using UnityEngine;

public static class MouseUtil
{
    private static Camera _cachedCamera;

    // A smart property that ensures we always have a valid camera
    private static Camera CurrentCamera
    {
        get
        {
            // If the cached camera is null or has been destroyed, find the new Main Camera
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
            }
            return _cachedCamera;
        }
    }

    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f)
    {
        // Use CurrentCamera instead of the variable directly
        // This prevents NullReferenceExceptions if the scene changes
        if (CurrentCamera == null)
        {
            Debug.LogError("No Main Camera found in the scene!");
            return Vector3.zero;
        }

        Plane dragPlane = new(CurrentCamera.transform.forward, new Vector3(0, 0, zValue));
        Ray ray = CurrentCamera.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}