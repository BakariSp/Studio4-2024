using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MountainGenerator : MonoBehaviour
{
    [SerializeField] private Material mountainMaterial;
    [SerializeField] private float projectionRadius = 100f;
    [SerializeField] private int segmentsPerUnit = 2;
    [SerializeField] private float noiseScale = 0.3f;
    [SerializeField] private float baseWidth = 20f;

    [SerializeField] private bool isMountainModeOn = false;
    [SerializeField] private TextMeshProUGUI modeStatusText;

    [SerializeField] private Camera userCamera;

    public GameObject GenerateMountain(ShapeDrawingEvent drawingEvent)
    {
        if (drawingEvent.Points.Count < 2) return null;

        // Create mountain parent object
        GameObject mountainObject = new GameObject("Mountain");
        
        // Generate mesh for the mountain
        Mesh mountainMesh = CreateMountainMesh(drawingEvent.Points, drawingEvent.MountainHeight);
        
        // Add mesh components
        MeshFilter meshFilter = mountainObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = mountainObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = mountainObject.AddComponent<MeshCollider>();
        
        // Assign components
        meshFilter.mesh = mountainMesh;
        meshCollider.sharedMesh = mountainMesh;
        meshRenderer.material = mountainMaterial;
        
        return mountainObject;
    }

    private Mesh CreateMountainMesh(List<Vector3> ridgeLine, float height)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Project ridge line points to terrain distance
        List<Vector3> projectedPoints = ProjectPointsToDistance(ridgeLine);
        
        // Generate vertices along the mountain ridge
        for (int i = 0; i < projectedPoints.Count; i++)
        {
            Vector3 point = projectedPoints[i];
            Vector3 direction = (i < projectedPoints.Count - 1) 
                ? (projectedPoints[i + 1] - point).normalized 
                : (point - projectedPoints[i - 1]).normalized;
            Vector3 cross = Vector3.Cross(direction, Vector3.up).normalized;

            // Create multiple height points for each segment
            int heightPoints = 5; // Number of points across the ridge
            for (int h = 0; h < heightPoints; h++)
            {
                float t = h / (float)(heightPoints - 1);
                float localHeight = height * (0.5f + 0.5f * Mathf.PerlinNoise(point.x * noiseScale + t, point.z * noiseScale));
                
                // Create base points and ridge points
                Vector3 baseLeft = point + cross * baseWidth;
                Vector3 baseRight = point - cross * baseWidth;
                Vector3 ridgePoint = point + Vector3.up * localHeight;
                
                // Add noise to the ridge point
                ridgePoint += Random.insideUnitSphere * (height * 0.1f);
                
                vertices.Add(baseLeft);
                vertices.Add(ridgePoint);
                vertices.Add(baseRight);
            }
        }

        // Create triangles connecting the segments
        for (int i = 0; i < projectedPoints.Count - 1; i++)
        {
            for (int h = 0; h < 4; h++) // heightPoints - 1
            {
                int baseIndex = i * 15 + h * 3; // 5 height points * 3 vertices per point
                
                // Connect to next segment
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 3);

                triangles.Add(baseIndex + 3);
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 4);

                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 4);

                triangles.Add(baseIndex + 4);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 5);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private List<Vector3> ProjectPointsToDistance(List<Vector3> points)
    {
        List<Vector3> projectedPoints = new List<Vector3>();
        
        if (userCamera == null)
        {
            userCamera = Camera.main;
            if (userCamera == null) return projectedPoints;
        }

        Vector3 cameraPosition = userCamera.transform.position;
        
        // Find the center point of the drawn line
        Vector3 lineCenter = Vector3.zero;
        foreach (Vector3 point in points)
        {
            lineCenter += point;
        }
        lineCenter /= points.Count;

        // Project the center point to get the mountain's center position
        Vector3 directionToCenter = (lineCenter - cameraPosition).normalized;
        Vector3 horizontalDirection = new Vector3(directionToCenter.x, 0, directionToCenter.z).normalized;
        Vector3 mountainCenter = cameraPosition + (horizontalDirection * projectionRadius);

        // Calculate the scale of the mountain relative to the drawn line
        float drawnLineWidth = 0f;
        for (int i = 0; i < points.Count - 1; i++)
        {
            drawnLineWidth += Vector3.Distance(points[i], points[i + 1]);
        }

        // For each point, maintain its relative position to the center
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 pointOffset = points[i] - lineCenter;
            // Keep the X and Z offset proportions but set Y to 0
            Vector3 horizontalOffset = new Vector3(pointOffset.x, 0, pointOffset.z);
            
            // Project the point while maintaining its relative position
            Vector3 projectedPoint = mountainCenter + horizontalOffset;
            projectedPoint.y = 0; // Keep at ground level
            projectedPoints.Add(projectedPoint);
        }

        return projectedPoints;
    }

    public bool IsMountainModeOn()
    {
        return isMountainModeOn;
    }

    public void SetMountainMode(bool state)
    {
        isMountainModeOn = state;
        if (modeStatusText != null)
        {
            modeStatusText.text = isMountainModeOn ? "Mountain Mode: ON" : "Mountain Mode: OFF";
        }
    }

    public void ToggleMountainMode()
    {
        isMountainModeOn = !isMountainModeOn;
        SetMountainMode(isMountainModeOn);
    }
}