using UnityEngine;
using System.Collections.Generic;

public class DeleteControl : MonoBehaviour
{
    [SerializeField] private Camera userCamera;
    [SerializeField] private LayerMask deletableLayerMask;
    [SerializeField] private bool isDeleteModeOn = false;
    [SerializeField] private float projectionDistance = 100f;
    [SerializeField] private float boxWidth = 1f; // Width of the deletion box
    [SerializeField] private Color boxColor = new Color(1f, 0f, 0f, 0.5f); // Red with transparency
    [SerializeField] private float debugLineDuration = 2f; // How long the debug lines stay visible

    private void Start()
    {
        if (userCamera == null)
            userCamera = Camera.main;
    }

    private void DebugLog(string message)
    {
        if (!isDeleteModeOn) return;
        
        Debug.Log($"DeleteControl: {message}");
        if (DebugDisplay.Instance != null)
            DebugDisplay.Instance.AddDebugMessage($"DeleteControl: {message}");
        if (GeneratorUIController.Instance != null)
            GeneratorUIController.Instance.UpdateDebugInfo($"DeleteControl: {message}");
    }

    private void DrawDebugBox(Vector3[] corners)
    {
        // Draw bottom rectangle
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(corners[i], corners[(i + 1) % 4], boxColor, debugLineDuration);
        }

        // Draw top rectangle
        for (int i = 4; i < 8; i++)
        {
            Debug.DrawLine(corners[i], corners[(i + 1) % 4 + 4], boxColor, debugLineDuration);
        }

        // Draw vertical lines connecting top and bottom
        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(corners[i], corners[i + 4], boxColor, debugLineDuration);
        }
    }

    public void ProcessDeleteArea(ShapeDrawingEvent shapeEvent)
{
    if (!isDeleteModeOn)
        return;

    List<Vector3> points = shapeEvent.Points;
    if (points.Count < 2)
    {
        DebugLog("Not enough points for deletion");
        return;
    }

    HashSet<GameObject> objectsToDelete = new HashSet<GameObject>();

    // Process each line segment
    for (int i = 0; i < points.Count - 1; i++)
    {
        Vector3 startPoint = ProjectToWorld(points[i]);
        Vector3 endPoint = ProjectToWorld(points[i + 1]);
        
        Vector3 direction = (endPoint - startPoint).normalized;
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * boxWidth;
        
        // Define box corners
        Vector3[] corners = new Vector3[]
        {
            startPoint + right,                    // Bottom front right
            startPoint - right,                    // Bottom front left
            endPoint - right,                      // Bottom back left
            endPoint + right,                      // Bottom back right
            startPoint + right + Vector3.up * boxWidth,  // Top front right
            startPoint - right + Vector3.up * boxWidth,  // Top front left
            endPoint - right + Vector3.up * boxWidth,    // Top back left
            endPoint + right + Vector3.up * boxWidth     // Top back right
        };

        // Visualize the box
        DrawDebugBox(corners);

        // Create bounds and check for objects to delete
        Bounds deletionBounds = new Bounds(startPoint, Vector3.zero);
        foreach (Vector3 corner in corners)
        {
            deletionBounds.Encapsulate(corner);
        }

        // Find objects within bounds
        Collider[] colliders = Physics.OverlapBox(
            deletionBounds.center,
            deletionBounds.extents,
            Quaternion.LookRotation(direction),
            deletableLayerMask
        );

        foreach (Collider collider in colliders)
        {
            if (collider != null && collider.gameObject != null)
            {
                objectsToDelete.Add(collider.gameObject);
                DebugLog($"Found object to delete: {collider.gameObject.name}");
            }
        }
    }

    // Delete all found objects
    int deleteCount = objectsToDelete.Count;
    foreach (GameObject obj in objectsToDelete)
    {
        DebugLog($"Deleting object: {obj.name}");
        Destroy(obj);
    }
    DebugLog($"Deleted {deleteCount} objects");
}

    private Vector3 ProjectToWorld(Vector3 screenPoint)
    {
        Ray ray = userCamera.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, projectionDistance, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        
        // If no ground hit, project to a plane at a fixed distance
        return ray.origin + ray.direction * projectionDistance;
    }

    public void SetDeleteMode(bool state)
    {
        isDeleteModeOn = state;
        DebugLog($"Delete mode: {(isDeleteModeOn ? "ON" : "OFF")}");
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Delete Mode: {(isDeleteModeOn ? "ON" : "OFF")}");
        }
    }

    public void ToggleDeleteMode()
    {
        SetDeleteMode(!isDeleteModeOn);
    }

    public bool IsDeleteModeOn()
    {
        return isDeleteModeOn;
    }
}
