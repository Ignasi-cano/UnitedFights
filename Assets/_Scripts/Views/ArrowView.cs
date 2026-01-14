using UnityEngine;

public class ArrowView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int positionCount = 50;
    [SerializeField] private float heightFactor = 0.2f;

    [SerializeField] private GameObject arrowHead;
    [SerializeField] private LineRenderer lineRenderer;
    private Vector3 startPosition;

    private void Update()
    {
        Vector3 endPosition = MouseUtil.GetMousePositionInWorldSpace();
        UpdateCurve(endPosition);
    }

    public void SetupArrow(Vector3 startPosition)
    {
        this.startPosition = startPosition;
        lineRenderer.positionCount = positionCount;
        UpdateCurve(MouseUtil.GetMousePositionInWorldSpace());
    }

    private void UpdateCurve(Vector3 endPosition)
    {
        // Calculate control point for the quadratic Bezier curve
        Vector3 midPoint = (startPosition + endPosition) / 4f;
        float distance = Vector3.Distance(startPosition, endPosition);
        Vector3 controlPoint = midPoint + Vector3.up * (distance * heightFactor);

        Vector3[] points = new Vector3[positionCount];
        for (int i = 0; i < positionCount; i++)
        {
            float t = i / (float)(positionCount - 1);
            points[i] = CalculateQuadraticBezierPoint(t, startPosition, controlPoint, endPosition);
        }

        lineRenderer.SetPositions(points);

        // Position and rotate arrow head
        arrowHead.transform.position = endPosition;
       
        // Calculate direction from the slightly previous point on the curve to the end point for better rotation
        Vector3 pointBeforeEnd = CalculateQuadraticBezierPoint(0.95f, startPosition, controlPoint, endPosition);
        Vector3 direction = (endPosition - pointBeforeEnd).normalized;
        arrowHead.transform.right = direction;
    }

    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        return (uu * p0) + (2 * u * t * p1) + (tt * p2);
    }
}