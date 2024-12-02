using UnityEngine;
using System.Collections.Generic;

public class DeleteGenerator : MonoBehaviour
{
    [SerializeField] private Camera userCamera;
    [SerializeField] private LayerMask deletableLayerMask;
    [SerializeField] private bool isDeleteModeOn = false;
    [SerializeField] private float projectionDistance = 100f;
    [SerializeField] private float boxWidth = 1f;
    [SerializeField] private Color boxColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float debugLineDuration = 2f;

    private void Start()
    {
        if (userCamera == null)
            userCamera = Camera.main;
    }

    private void DebugLog(string message)
    {
        if (!isDeleteModeOn) return;
        
        Debug.Log($"DeleteGenerator: {message}");
        if (DebugDisplay.Instance != null)
            DebugDisplay.Instance.AddDebugMessage($"DeleteGenerator: {message}");
        if (GeneratorUIController.Instance != null)
            GeneratorUIController.Instance.UpdateDebugInfo($"DeleteGenerator: {message}");
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

    public int ProcessDeleteArea(ShapeDrawingEvent shapeEvent)
    {
        List<Vector3> points = shapeEvent.Points;
        if (points.Count < 2)
        {
            DebugLog("Not enough points for deletion");
            return 0;
        }

        HashSet<GameObject> objectsToDelete = new HashSet<GameObject>();

        // Process each line segment
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 startPoint = ProjectToWorld(points[i]);
            Vector3 endPoint = ProjectToWorld(points[i + 1]);
            
            Vector3 direction = (endPoint - startPoint).normalized;
            Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * boxWidth;
            
            Vector3[] corners = CreateBoxCorners(startPoint, endPoint, right);
            DrawDebugBox(corners);

            // Find and collect objects to delete
            Bounds deletionBounds = CreateBoundsFromCorners(corners);
            CollectObjectsToDelete(deletionBounds, direction, objectsToDelete);
        }

        // Delete all found objects
        int deleteCount = objectsToDelete.Count;
        foreach (GameObject obj in objectsToDelete)
        {
            DebugLog($"Deleting object: {obj.name}");
            Destroy(obj);
        }

        return deleteCount;
    }

    private Vector3[] CreateBoxCorners(Vector3 startPoint, Vector3 endPoint, Vector3 right)
    {
        return new Vector3[]
        {
            startPoint + right,
            startPoint - right,
            endPoint - right,
            endPoint + right,
            startPoint + right + Vector3.up * boxWidth,
            startPoint - right + Vector3.up * boxWidth,
            endPoint - right + Vector3.up * boxWidth,
            endPoint + right + Vector3.up * boxWidth
        };
    }

    private Bounds CreateBoundsFromCorners(Vector3[] corners)
    {
        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        foreach (Vector3 corner in corners)
        {
            bounds.Encapsulate(corner);
        }
        return bounds;
    }

    private void CollectObjectsToDelete(Bounds bounds, Vector3 direction, HashSet<GameObject> objectsToDelete)
    {
        Collider[] colliders = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
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

    private Vector3 ProjectToWorld(Vector3 screenPoint)
    {
        Ray ray = userCamera.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, projectionDistance, LayerMask.GetMask("Ground")))
        {
            return hit.point;
        }
        
        return ray.origin + ray.direction * projectionDistance;
    }

    public bool IsDeleteModeOn() => isDeleteModeOn;

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
} 