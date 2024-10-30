using UnityEngine;
using System.Collections.Generic;
public class DoorProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private DoorSurfaceGenerator doorGenerator;
    [SerializeField] private Camera userCamera;
    private List<GameObject> allCreatedLines = new List<GameObject>();
    
    [Header("Door Detection Settings")]
    [Range(0, 90)]
    public float maxDoorAngle = 30f;
    public float minDoorAspectRatio = 0.8f;
    public float maxDoorAspectRatio = 4.0f;

    private void DebugLog(string message)
    {
        Debug.Log(message);
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage(message);
        }
    }

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        DebugLog($"Processing shape: {shapeEvent.RecognizedShape}");
        
        if (shapeEvent.RecognizedShape != ShapeType.Rectangle)
        {
            DebugLog("Shape is not a rectangle, skipping...");
            return;
        }
        
        if (doorGenerator == null)
        {
            DebugLog("DoorGenerator is not assigned!");
            return;
        }

        if (userCamera == null)
        {
            DebugLog("UserCamera is not assigned!");
            return;
        }

        bool isDoor = IsLikelyDoor(shapeEvent.Points);
        DebugLog($"Is likely door? {isDoor}");
        
        if (isDoor)
        {
            DebugLog("Shape recognized as door, generating surface...");
            GenerateDoorSurface(shapeEvent.LineRenderer);
        }
        else
        {
            Vector3 size = CalculateRectangleSizeInCameraSpace(shapeEvent.Points, userCamera);
            float aspectRatio = size.y / size.x;
            DebugLog($"Rectangle validation failed:");
            DebugLog($"- Aspect Ratio: {aspectRatio:F2} (should be between {minDoorAspectRatio} and {maxDoorAspectRatio})");
            DebugLog($"- Size: Width={size.x:F2}, Height={size.y:F2}");
        }
    }

    private bool IsLikelyDoor(List<Vector3> points)
    {
        if (points.Count < 4)
        {
            DebugLog("Not enough points for door detection");
            return false;
        }

        // Calculate the average normal of the rectangle using the camera's view direction
        Vector3 viewDirection = (userCamera.transform.position - points[0]).normalized;
        Vector3 avgNormal = Vector3.zero;
        
        // Use the first four points to get a more stable normal
        for (int i = 0; i < 3; i++)
        {
            Vector3 edge1 = points[i + 1] - points[i];
            Vector3 edge2 = points[i + 2] - points[i + 1];
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;
            
            // Make sure the normal is facing towards the camera
            if (Vector3.Dot(localNormal, viewDirection) < 0)
            {
                localNormal = -localNormal;
            }
            avgNormal += localNormal;
        }
        avgNormal.Normalize();

        // Check if the rectangle is roughly vertical
        float angleWithUp = Vector3.Angle(avgNormal, Vector3.up);
        bool isVertical = angleWithUp < maxDoorAngle || angleWithUp > (180 - maxDoorAngle);

        // Check if the rectangle is roughly facing the camera
        float dotWithView = Mathf.Abs(Vector3.Dot(avgNormal, viewDirection));
        bool isFacingCamera = dotWithView > 0.5f;

        // Calculate size in camera space
        Vector3 size = CalculateRectangleSizeInCameraSpace(points, userCamera);
        float aspectRatio = size.y / size.x;
        bool hasValidAspectRatio = aspectRatio >= minDoorAspectRatio && aspectRatio <= maxDoorAspectRatio;

        DebugLog($"Door validation details:");
        DebugLog($"- Angle with up: {angleWithUp:F1}°");
        DebugLog($"- Is vertical: {isVertical}");
        DebugLog($"- Is facing camera: {isFacingCamera} (dot: {dotWithView:F2})");
        DebugLog($"- Aspect ratio: {aspectRatio:F2} (valid: {hasValidAspectRatio})");
        DebugLog($"- Size in camera space: Width={size.x:F2}, Height={size.y:F2}");

        return (isVertical || isFacingCamera) && hasValidAspectRatio;
    }

    private Vector3 CalculateRectangleSizeInCameraSpace(List<Vector3> points, Camera camera)
    {
        if (points.Count < 4) return Vector3.zero;

        // Project points to camera space
        List<Vector2> screenPoints = new List<Vector2>();
        foreach (Vector3 point in points)
        {
            Vector2 screenPoint = camera.WorldToScreenPoint(point);
            screenPoints.Add(screenPoint);
        }

        // Find the bounds in screen space
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector2 point in screenPoints)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        // Convert screen space size to world space size at the door's distance
        Vector3 centerPoint = points[0];
        float distanceToCamera = Vector3.Distance(camera.transform.position, centerPoint);
        
        float width = (maxX - minX) * distanceToCamera / camera.pixelWidth;
        float height = (maxY - minY) * distanceToCamera / camera.pixelHeight;

        return new Vector3(width, height, 0);
    }

    private void GenerateDoorSurface(LineRenderer rectangleRenderer)
    {
        if (doorGenerator == null || userCamera == null)
        {
            Debug.LogError("DoorProcessor: Missing required components for door generation");
            return;
        }

        Vector3[] positions = new Vector3[rectangleRenderer.positionCount];
        rectangleRenderer.GetPositions(positions);

        GameObject doorSurface = doorGenerator.GenerateDoorSurface(positions, userCamera);
        if (doorSurface != null)
        {
            Debug.Log("DoorProcessor: Door surface generated successfully");
            allCreatedLines.Add(doorSurface);
        }
        else
        {
            Debug.LogError("DoorProcessor: Failed to generate door surface");
        }
    }
} 