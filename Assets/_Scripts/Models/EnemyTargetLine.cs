using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class EnemyTargetLine : MonoBehaviour
{
    [SerializeField] private Vector3 startOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 endOffset = new Vector3(0f, 0.5f, 0f);

    private LineRenderer lineRenderer;
    private Transform source;
    private Transform target;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(Color.red, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0.35f, 0f),
                new GradientAlphaKey(0.35f, 1f)
            }
        );

        lineRenderer.colorGradient = gradient;
        lineRenderer.startWidth = 0.06f;
        lineRenderer.endWidth = 0.06f;
        lineRenderer.sortingOrder = 200;
    }

    public void Setup(Transform sourceTransform, Transform targetTransform)
    {
        source = sourceTransform;
        target = targetTransform;
        gameObject.SetActive(source != null && target != null);
        UpdateLine();
    }

    private void LateUpdate()
    {
        UpdateLine();
    }

    private void UpdateLine()
    {
        if (lineRenderer == null || source == null || target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        CombatantView targetCombatant = target.GetComponent<CombatantView>();
        if (targetCombatant != null && (targetCombatant.CurrentHealth <= 0 || targetCombatant.IsDying))
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        lineRenderer.SetPosition(0, source.position + startOffset);
        lineRenderer.SetPosition(1, target.position + endOffset);
    }
}