using System.Collections.Generic;
using UnityEngine;

public class DynamicLineCollider : MonoBehaviour
{
    public LineRenderer lineRenderer;
    private List<BoxCollider> boxColliders = new List<BoxCollider>();

    void Start()
    {
        UpdateLineColliders();
    }

    void Update()
    {
        UpdateLineColliders();
    }

    void UpdateLineColliders()
    {
        int segmentCount = lineRenderer.positionCount - 1;

        // Create additional colliders if necessary
        while (boxColliders.Count < segmentCount)
        {
            BoxCollider newCollider = new GameObject("LineSegmentCollider").AddComponent<BoxCollider>();
            newCollider.transform.parent = transform; // Attach to the LineRenderer object
            boxColliders.Add(newCollider);
        }

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 start = lineRenderer.GetPosition(i);
            Vector3 end = lineRenderer.GetPosition(i + 1);

            // Position the collider in the middle of the line segment
            BoxCollider boxCollider = boxColliders[i];
            boxCollider.gameObject.SetActive(true); // Ensure the collider is active
            boxCollider.transform.position = (start + end) / 2;

            // Rotate collider to align with the segment direction
            Vector3 direction = end - start;
            boxCollider.transform.rotation = Quaternion.LookRotation(direction);

            // Adjust the collider's size
            boxCollider.size = new Vector3(0.05f, 0.05f, direction.magnitude); // Adjust thickness as needed
        }

        // Disable extra colliders if the line has fewer segments
        for (int i = segmentCount; i < boxColliders.Count; i++)
        {
            boxColliders[i].gameObject.SetActive(false);
        }
    }
}
