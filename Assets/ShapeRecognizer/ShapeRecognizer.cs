using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class ShapeRecognizer : MonoBehaviour
{
    public TextMeshProUGUI shapeText;
    public TextMeshProUGUI debugText;
    private List<Vector3> points = new List<Vector3>();
    
    [SerializeField] private float angleTolerance = 15f;
    [SerializeField] private float distanceTolerance = 0.2f;
    [SerializeField] private float radiusTolerance = 0.25f;

    private string debugInfo = "";

    public void RecognizeShape(LineRenderer lineRenderer)
    {
        points.Clear();
        debugInfo = "";
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            points.Add(lineRenderer.GetPosition(i));
        }

        debugInfo += $"Total Points: {points.Count}\n";

        // Check if shape is open
        float startEndDistance = Vector3.Distance(points[0], points[points.Count - 1]);
        debugInfo += $"Start-End Distance: {startEndDistance:F3}\n";

        if (startEndDistance > distanceTolerance)
        {
            Debug.Log("Open Shape Detected");
            if (shapeText != null)
                shapeText.text = "Shape: Open Shape";
            if (debugText != null)
                debugText.text = debugInfo;
            return;
        }

        if (IsCircle(points))
        {
            Debug.Log("Circle Detected");
            if (shapeText != null)
                shapeText.text = "Shape: Circle";
            if (debugText != null)
                debugText.text = debugInfo;
        }
        else if (IsRectangle(points))
        {
            Debug.Log("Rectangle Detected");
            if (shapeText != null)
                shapeText.text = "Shape: Rectangle";
            if (debugText != null)
                debugText.text = debugInfo;
        }
        else if (IsTriangle(points))
        {
            Debug.Log("Triangle Detected");
            if (shapeText != null)
                shapeText.text = "Shape: Triangle";
            if (debugText != null)
                debugText.text = debugInfo;
        }
        else
        {
            if (shapeText != null)
                shapeText.text = "Shape: Unknown";
            if (debugText != null)
                debugText.text = debugInfo;
        }
    }

    bool IsRectangle(List<Vector3> points)
    {
        if (points.Count < 4) return false;

        // Simplify points to get main corners
        List<Vector3> corners = SimplifyPoints(points, 4);
        if (corners.Count != 4) return false;

        // Calculate angles at each corner
        List<float> angles = new List<float>();
        for (int i = 0; i < 4; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[(i + 1) % 4];
            Vector3 prev = corners[(i + 3) % 4];

            Vector3 dir1 = (next - current).normalized;
            Vector3 dir2 = (prev - current).normalized;
            float angle = Vector3.Angle(dir1, dir2);
            angles.Add(angle);
        }

        // Check for right angles (90 degrees)
        foreach (float angle in angles)
        {
            if (Mathf.Abs(angle - 90) > angleTolerance)
                return false;
        }

        // Check if opposite sides are approximately equal
        float[] sides = new float[4];
        for (int i = 0; i < 4; i++)
        {
            sides[i] = Vector3.Distance(corners[i], corners[(i + 1) % 4]);
        }

        return Mathf.Abs(sides[0] - sides[2]) < distanceTolerance &&
               Mathf.Abs(sides[1] - sides[3]) < distanceTolerance;
    }

    bool IsTriangle(List<Vector3> points)
    {
        if (points.Count < 3) return false;

        // Simplify points to get main corners
        List<Vector3> corners = SimplifyPoints(points, 3);
        if (corners.Count != 3) return false;

        // Calculate angles
        float[] angles = new float[3];
        for (int i = 0; i < 3; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[(i + 1) % 3];
            Vector3 prev = corners[(i + 2) % 3];

            Vector3 dir1 = (next - current).normalized;
            Vector3 dir2 = (prev - current).normalized;
            angles[i] = Vector3.Angle(dir1, dir2);
        }

        // Sum of angles in a triangle should be close to 180 degrees
        float angleSum = angles[0] + angles[1] + angles[2];
        return Mathf.Abs(angleSum - 180) < angleTolerance;
    }

    bool IsCircle(List<Vector3> points)
    {
        if (points.Count < 10)
        {
            debugInfo += "Failed: Not enough points for circle\n";
            return false;
        }

        // Calculate center point
        Vector3 center = Vector3.zero;
        foreach (var point in points)
        {
            center += point;
        }
        center /= points.Count;
        debugInfo += $"Center: {center:F2}\n";

        // Calculate average radius and variation
        float averageRadius = 0;
        List<float> radiusVariations = new List<float>();
        
        foreach (var point in points)
        {
            float radius = Vector3.Distance(point, center);
            averageRadius += radius;
        }
        averageRadius /= points.Count;
        debugInfo += $"Average Radius: {averageRadius:F3}\n";

        // Calculate radius variations
        float maxVariation = 0;
        float totalVariation = 0;
        
        foreach (var point in points)
        {
            float radius = Vector3.Distance(point, center);
            float variation = Mathf.Abs(radius - averageRadius) / averageRadius;
            maxVariation = Mathf.Max(maxVariation, variation);
            totalVariation += variation;
            radiusVariations.Add(variation);
        }

        float averageVariation = totalVariation / points.Count;
        debugInfo += $"Max Radius Variation: {maxVariation:F3}\n";
        debugInfo += $"Average Variation: {averageVariation:F3}\n";
        debugInfo += $"Tolerance: {radiusTolerance:F3}\n";

        // Consider both max variation and average variation
        bool isCircle = maxVariation <= radiusTolerance && averageVariation <= (radiusTolerance * 0.5f);
        debugInfo += isCircle ? "Result: Circle detected\n" : "Result: Not circular enough\n";
        
        return isCircle;
    }

    private List<Vector3> SimplifyPoints(List<Vector3> points, int targetCount)
    {
        List<Vector3> simplified = new List<Vector3>();
        float step = (float)points.Count / targetCount;
        
        for (int i = 0; i < targetCount; i++)
        {
            int index = Mathf.Min(Mathf.FloorToInt(i * step), points.Count - 1);
            simplified.Add(points[index]);
        }
        
        return simplified;
    }
}
