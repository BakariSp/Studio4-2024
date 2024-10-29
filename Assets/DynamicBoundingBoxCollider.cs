using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(BoxCollider))]
public class DynamicBoundingBoxCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private BoxCollider boxCollider;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        UpdateBoundingBox();
    }

    void UpdateBoundingBox()
    {
        if (lineRenderer.positionCount < 2)
        {
            boxCollider.enabled = false; // Disable collider if there are less than 2 points
            return;
        }

        boxCollider.enabled = true;

        // Calculate the bounding box based on line segment points
        Vector3 min = lineRenderer.GetPosition(0);
        Vector3 max = lineRenderer.GetPosition(0);

        for (int i = 1; i < lineRenderer.positionCount; i++)
        {
            Vector3 pos = lineRenderer.GetPosition(i);
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }

        // Set the center and size of the BoxCollider based on the bounding box
        boxCollider.center = (min + max) / 2;
        boxCollider.size = max - min;
    }
}
