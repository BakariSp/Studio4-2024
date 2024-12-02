using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class WallGenerator : MonoBehaviour
{
    [SerializeField] private Material wallMaterial;
    [SerializeField] private float maxProjectionRadius = 100f;
    // [SerializeField] private float minProjectionRadius = 5f;
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float wallThickness = 0.5f;
    [SerializeField] private bool isWallModeOn = false;
    [SerializeField] private bool isStraightMode = true;
    [SerializeField] private Camera userCamera;
    [SerializeField] private float smoothingFactor = 0.5f;
    [Header("Layer Settings")]
    [SerializeField] private LayerMask generatedObjectLayer;

    public bool IsStraightMode() => isStraightMode;
    
    private void DebugLog(string message)
    {
        Debug.Log($"TreeGenerator: {message}");
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateDebugInfo($"TreeGenerator: {message}");
        }
    }

    public void SetStraightMode(bool straight)
    {
        isStraightMode = straight;
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Wall Mode: {(isWallModeOn ? "ON" : "OFF")}\nType: {(straight ? "Straight Only" : "Free Draw")}");
        }
    }

    public void ToggleStraightMode()
    {
        isStraightMode = !isStraightMode;
        SetStraightMode(isStraightMode);
    }

    public GameObject GenerateWall(ShapeDrawingEvent drawingEvent)
    {
        DebugLog("GenerateWall called");
        
        if (drawingEvent.Points.Count < 2)
        {
            DebugLog($"Not enough points for wall: {drawingEvent.Points.Count}");
            return null;
        }

        if (isStraightMode)
        {
            return GenerateStraightWall(drawingEvent.Points);
        }
        
        if (drawingEvent.IsShapeClosed)
        {
            return GenerateWallSurface(drawingEvent.Points);
        }
        else
        {
            return GenerateFreeformWall(drawingEvent.Points);
        }
    }

    private GameObject GenerateStraightWall(List<Vector3> points)
    {
        Vector3 startPoint = points[0];
        Vector3 endPoint = points[points.Count - 1];
        
        Vector3 projectedStartPoint = ProjectPointToSurface(startPoint);
        Vector3 projectedEndPoint = ProjectPointToSurface(endPoint);
        
        return InstantiateWall(projectedStartPoint, projectedEndPoint);
    }

    private GameObject GenerateFreeformWall(List<Vector3> points)
    {
        List<Vector3> projectedPoints = new List<Vector3>();
        foreach (Vector3 point in points)
        {
            projectedPoints.Add(ProjectPointToSurface(point));
        }

        List<Vector3> smoothedPoints = SmoothPoints(projectedPoints);

        GameObject wallObj = new GameObject("FreeformWall");
        MeshFilter meshFilter = wallObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wallObj.AddComponent<MeshRenderer>();
        meshRenderer.material = wallMaterial;

        Mesh mesh = CreateWallMesh(smoothedPoints);
        meshFilter.mesh = mesh;

        MeshCollider collider = wallObj.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        return wallObj;
    }

    private GameObject GenerateWallSurface(List<Vector3> points)
    {
        List<Vector3> projectedPoints = new List<Vector3>();
        foreach (Vector3 point in points)
        {
            projectedPoints.Add(ProjectPointToSurface(point));
        }

        GameObject surfaceObj = new GameObject("WallSurface");
        MeshFilter meshFilter = surfaceObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = surfaceObj.AddComponent<MeshRenderer>();
        meshRenderer.material = wallMaterial;

        Mesh mesh = CreateSurfaceMesh(projectedPoints);
        meshFilter.mesh = mesh;

        MeshCollider collider = surfaceObj.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        return surfaceObj;
    }

    private List<Vector3> SmoothPoints(List<Vector3> points)
    {
        List<Vector3> smoothedPoints = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            if (i > 0 && i < points.Count - 1)
            {
                Vector3 prev = points[i - 1];
                Vector3 next = points[i + 1];
                point = Vector3.Lerp(point, (prev + next) / 2f, smoothingFactor);
            }
            smoothedPoints.Add(point);
        }
        return smoothedPoints;
    }

    private Mesh CreateWallMesh(List<Vector3> points)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < points.Count; i++)
        {
            vertices.Add(points[i]);
            vertices.Add(points[i] + Vector3.up * wallHeight);
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            int bottomLeft = i * 2;
            int bottomRight = (i + 1) * 2;
            int topLeft = bottomLeft + 1;
            int topRight = bottomRight + 1;

            triangles.Add(bottomLeft);
            triangles.Add(topLeft);
            triangles.Add(bottomRight);

            triangles.Add(bottomRight);
            triangles.Add(topLeft);
            triangles.Add(topRight);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh CreateSurfaceMesh(List<Vector3> points)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Vector3 point in points)
        {
            vertices.Add(point);
        }

        for (int i = 1; i < points.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
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
        if (Physics.Raycast(userCamera.transform.position, directionToPoint, out hit, maxProjectionRadius, groundLayer))
        {
            DebugLog($"Raycast hit ground at: {hit.point}");
            return hit.point;
        }
        
        Vector3 maxDistancePoint = userCamera.transform.position + (directionToPoint * maxProjectionRadius);
        DebugLog($"Raycast missed ground layer, placing at max distance: {maxDistancePoint}");
        return maxDistancePoint;
    }

    private GameObject InstantiateWall(Vector3 startPoint, Vector3 endPoint)
    {
        try
        {
            Vector3 wallDirection = endPoint - startPoint;
            float wallLength = wallDirection.magnitude;
            Vector3 wallCenter = startPoint + (wallDirection / 2f);
            
            Quaternion wallRotation = Quaternion.LookRotation(wallDirection);
            
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.position = wallCenter;
            wall.transform.rotation = wallRotation;
            
            Vector3 wallScale = new Vector3(wallThickness, wallHeight, wallLength);
            wall.transform.localScale = wallScale;
            
            // Replace the default capsule collider with a box collider
            Destroy(wall.GetComponent<Collider>()); // Remove default collider
            BoxCollider boxCollider = wall.AddComponent<BoxCollider>();
            
            // Set the layer for the wall
            SetObjectLayer(wall, generatedObjectLayer);
            
            if (wallMaterial != null)
            {
                MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
                renderer.material = wallMaterial;
            }
            else
            {
                DebugLog("Warning: Wall material is not assigned!");
            }
            
            DebugLog($"Wall created between {startPoint} and {endPoint}");
            return wall;
        }
        catch (System.Exception e)
        {
            DebugLog($"Error instantiating wall: {e.Message}");
            return null;
        }
    }

    public bool IsWallModeOn() => isWallModeOn;

    public void SetWallMode(bool state)
    {
        isWallModeOn = state;
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Wall Mode: {(state ? "ON" : "OFF")}\nType: {(isStraightMode ? "Straight Only" : "Free Draw")}");
        }
    }

    public void ToggleWallMode()
    {
        isWallModeOn = !isWallModeOn;
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Wall Mode: {(isWallModeOn ? "ON" : "OFF")}\nType: {(isStraightMode ? "Straight Only" : "Free Draw")}");
        }
    }

    private void SetObjectLayer(GameObject obj, LayerMask layer)
    {
        if (obj == null) return;
        
        // Convert LayerMask to layer index
        int layerIndex = (int)Mathf.Log(layer.value, 2);
        
        // Set layer for the object and all its children
        obj.layer = layerIndex;
        foreach (Transform child in obj.transform)
        {
            SetObjectLayer(child.gameObject, layer);
        }
    }
}
