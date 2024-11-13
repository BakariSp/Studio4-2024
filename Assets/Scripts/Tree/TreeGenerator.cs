using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class TreeGenerator : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private float maxProjectionRadius = 100f;
    [SerializeField] private float minProjectionRadius = 5f;
    [SerializeField] private bool isTreeModeOn = false;
    [SerializeField] private Camera userCamera;

    private void DebugLog(string message)
    {
        Debug.Log($"TreeGenerator: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"TreeGenerator: {message}");
        }
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateDebugInfo($"TreeGenerator: {message}");
        }
    }

    public GameObject GenerateTree(ShapeDrawingEvent drawingEvent)
    {
        DebugLog("GenerateTree called");
        
        if (drawingEvent.Points.Count < 3)
        {
            DebugLog($"Not enough points: {drawingEvent.Points.Count}");
            return null;
        }

        // Calculate center point of the triangle
        Vector3 centerPoint = Vector3.zero;
        foreach (Vector3 point in drawingEvent.Points)
        {
            centerPoint += point;
        }
        centerPoint /= drawingEvent.Points.Count;
        DebugLog($"Center point calculated: {centerPoint}");

        // Get projected point for tree placement
        Vector3 projectedPoint = ProjectPointToSurface(centerPoint);
        DebugLog($"Projected point: {projectedPoint}");
        
        // Create tree at projected point
        GameObject tree = InstantiateTree(projectedPoint);
        DebugLog($"Tree instantiated: {(tree != null ? "success" : "failed")}");
        return tree;
    }

    private Vector3 ProjectPointToSurface(Vector3 point)
    {
        if (userCamera == null)
        {
            userCamera = Camera.main;
            if (userCamera == null) return point;
        }

        // Calculate direction from camera to the point
        Vector3 directionToPoint = point - userCamera.transform.position;
        directionToPoint.Normalize(); // Normalize to get direction only
        
        // Perform raycast from camera towards the point, only hitting the Ground layer
        int groundLayer = LayerMask.GetMask("Ground");
        RaycastHit hit;
        if (Physics.Raycast(userCamera.transform.position, directionToPoint, out hit, maxProjectionRadius, groundLayer))
        {
            DebugLog($"Raycast hit ground at: {hit.point}");
            return hit.point;
        }
        
        // If raycast doesn't hit, place tree at maximum distance
        Vector3 maxDistancePoint = userCamera.transform.position + (directionToPoint * maxProjectionRadius);
        DebugLog($"Raycast missed ground layer, placing at max distance: {maxDistancePoint}");
        return maxDistancePoint;
    }

    private GameObject InstantiateTree(Vector3 position)
    {
        if (treePrefab == null)
        {
            DebugLog("Error: Tree prefab is not assigned!");
            return null;
        }

        try
        {
            // Create tree with random rotation around Y axis
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject tree = Instantiate(treePrefab, position, randomRotation);
            tree.name = "Tree";
            DebugLog($"Tree created at position: {position}");
            return tree;
        }
        catch (System.Exception e)
        {
            DebugLog($"Error instantiating tree: {e.Message}");
            return null;
        }
    }

    public bool IsTreeModeOn()
    {
        return isTreeModeOn;
    }

    public void SetTreeMode(bool state)
    {
        isTreeModeOn = state;
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus($"Tree Mode: {(state ? "ON" : "OFF")}");
        }
    }

    public void ToggleTreeMode()
    {
        isTreeModeOn = !isTreeModeOn;
        SetTreeMode(isTreeModeOn);
    }
}
