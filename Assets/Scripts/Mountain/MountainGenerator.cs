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
    [SerializeField] private Camera userCamera;

    [Header("Mode Settings")]
    [SerializeField] private bool isMountainModeOn = false;
    [SerializeField] private bool isTerrainMode = false;

    [Header("Terrain Modification")]
    [SerializeField] private Terrain targetTerrain;
    [SerializeField] private float brushRadius = 5f;
    [SerializeField] private float heightDelta = 0.1f;
    [SerializeField] private AnimationCurve brushFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("References")]
    [SerializeField] private GameObject fingerObject;
    [SerializeField] private RaycastController raycastController;

    public GameObject GenerateMountain(ShapeDrawingEvent drawingEvent)
    {
        if (drawingEvent.Points.Count < 2) return null;

        // Project points onto the cylinder
        List<Vector3> projectedPoints = ProjectPointsToCylinder(drawingEvent.Points);

        // Create mountain parent object
        GameObject mountainObject = new GameObject("Mountain");

        // Generate mesh for the mountain
        Mesh mountainMesh = CreateMountainMesh(projectedPoints, drawingEvent.MountainHeight);

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

        foreach (Vector3 point in points)
        {
            Vector3 directionToPoint = point - userCamera.transform.position;
            directionToPoint.Normalize();
            
            // Project point at fixed distance
            Vector3 projectedPoint = userCamera.transform.position + (directionToPoint * projectionRadius);
            projectedPoint.y = 0; // Keep at ground level
            projectedPoints.Add(projectedPoint);
        }

        return projectedPoints;
    }

    private List<Vector3> ProjectPointsToCylinder(List<Vector3> points)
    {
        List<Vector3> projectedPoints = new List<Vector3>();

        if (userCamera == null)
        {
            userCamera = Camera.main;
            if (userCamera == null) return projectedPoints;
        }

        if (points.Count == 0) return projectedPoints;

        // Project the first point to determine the cylinder radius
        Vector3 firstPoint = points[0];
        Vector3 directionToFirstPoint = firstPoint - userCamera.transform.position;
        float distanceToFirstPoint = new Vector2(directionToFirstPoint.x, directionToFirstPoint.z).magnitude;
        float cylinderRadius = distanceToFirstPoint;

        foreach (Vector3 point in points)
        {
            Vector3 direction = point - userCamera.transform.position;
            float currentDistance = new Vector2(direction.x, direction.z).magnitude;

            // Normalize the horizontal direction
            Vector3 horizontalDir = new Vector3(direction.x, 0, direction.z).normalized;

            // Project the point onto the cylinder's surface
            Vector3 projectedPoint = userCamera.transform.position + horizontalDir * cylinderRadius + Vector3.up * direction.y;

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
        if (state)
        {
            isTerrainMode = false;
        }
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Mode: {(isMountainModeOn ? "Mountain" : isTerrainMode ? "Terrain" : "OFF")}");
        }
    }

    public void ToggleMountainMode()
    {
        SetMountainMode(!isMountainModeOn);
    }

    public bool IsTerrainMode() => isTerrainMode;

    public void SetTerrainMode(bool state)
    {
        isTerrainMode = state;
        if (state)
        {
            isMountainModeOn = false;
        }
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Mode: {(isTerrainMode ? "Terrain" : isMountainModeOn ? "Mountain" : "OFF")}");
        }
    }

    public void ToggleTerrainMode()
    {
        SetTerrainMode(!isTerrainMode);
    }

    public void GenerateTerrainModification(ShapeDrawingEvent drawingEvent)
    {
        if (targetTerrain == null)
        {
            DebugLog("Missing terrain reference!");
            return;
        }

        if (userCamera == null)
        {
            userCamera = Camera.main;
            if (userCamera == null)
            {
                DebugLog("No camera found!");
                return;
            }
        }

        // Project points at fixed distance
        List<Vector3> projectedPoints = new List<Vector3>();
        foreach (Vector3 point in drawingEvent.Points)
        {
            Vector3 directionToPoint = point - userCamera.transform.position;
            directionToPoint.Normalize();
            
            Vector3 projectedPoint = userCamera.transform.position + (directionToPoint * projectionRadius);
            projectedPoint.y = 0; // Keep at ground level
            projectedPoints.Add(projectedPoint);
        }

        if (projectedPoints.Count == 0)
        {
            DebugLog("No points could be projected");
            return;
        }

        // Modify terrain
        TerrainData terrainData = targetTerrain.terrainData;
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;
        float[,] heights = terrainData.GetHeights(0, 0, heightmapWidth, heightmapHeight);

        // Get terrain height scale for proper height modification
        float terrainHeight = terrainData.size.y;
        float normalizedHeightDelta = heightDelta / terrainHeight; // Convert to normalized height

        foreach (Vector3 worldPoint in projectedPoints)
        {
            // Convert to terrain space
            Vector3 localPos = worldPoint - targetTerrain.transform.position;
            float normalizedX = Mathf.Clamp01(localPos.x / terrainData.size.x);
            float normalizedZ = Mathf.Clamp01(localPos.z / terrainData.size.z);
            
            int terrainX = Mathf.RoundToInt(normalizedX * (heightmapWidth - 1));
            int terrainZ = Mathf.RoundToInt(normalizedZ * (heightmapHeight - 1));
            
            // Calculate brush size in terrain space
            int brushSize = Mathf.RoundToInt(brushRadius * (heightmapWidth - 1) / terrainData.size.x);
            
            // Apply height modification
            for (int x = -brushSize; x <= brushSize; x++)
            {
                for (int z = -brushSize; z <= brushSize; z++)
                {
                    int sampleX = terrainX + x;
                    int sampleZ = terrainZ + z;
                    
                    if (sampleX >= 0 && sampleX < heightmapWidth && 
                        sampleZ >= 0 && sampleZ < heightmapHeight)
                    {
                        float distance = Mathf.Sqrt(x * x + z * z) / brushSize;
                        if (distance <= 1)
                        {
                            float falloff = brushFalloff.Evaluate(distance);
                            float currentHeight = heights[sampleZ, sampleX];
                            // Use normalized height delta for proper scaling
                            heights[sampleZ, sampleX] = Mathf.Lerp(currentHeight, 
                                currentHeight + normalizedHeightDelta, 
                                falloff);
                        }
                    }
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
        DebugLog($"Terrain modified with {projectedPoints.Count} projected points");
    }

    private void DebugLog(string message)
    {
        Debug.Log($"MountainGenerator: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"MountainGenerator: {message}");
        }
    }

    private Vector3 ProjectPointToSurface(Vector3 point)
    {
        if (userCamera == null)
        {
            userCamera = Camera.main;
            if (userCamera == null) return point;
        }

        Vector3 directionToPoint = point - userCamera.transform.position;
        directionToPoint.Normalize();
        
        int groundLayer = LayerMask.GetMask("Ground");
        RaycastHit hit;
        if (Physics.Raycast(userCamera.transform.position, directionToPoint, out hit, projectionRadius, groundLayer))
        {
            DebugLog($"Raycast hit ground at: {hit.point}");
            return hit.point;
        }
        
        Vector3 maxDistancePoint = userCamera.transform.position + (directionToPoint * projectionRadius);
        DebugLog($"Raycast missed ground layer, placing at max distance: {maxDistancePoint}");
        return maxDistancePoint;
    }
}