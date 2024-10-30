using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShapeRecognizer : MonoBehaviour
{
    [SerializeField] private float angleTolerance = 15f;
    [SerializeField] private float distanceTolerance = 0.2f;
    [SerializeField] private float radiusTolerance = 0.25f;
    [SerializeField] private float minPointsForShape = 10f; // Minimum points needed for shape recognition

    public bool IsRectangle(List<Vector3> points)
    {
        if (points.Count < minPointsForShape) 
        {
            DebugLog("Not enough points for rectangle");
            return false;
        }

        // Simplify points to get main corners
        List<Vector3> corners = SimplifyPoints(points, 4);
        if (corners.Count != 4)
        {
            DebugLog("Could not find 4 corners for rectangle");
            return false;
        }

        // Calculate angles at each corner and side lengths
        List<float> angles = new List<float>();
        List<float> sideLengths = new List<float>();
        
        for (int i = 0; i < 4; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[(i + 1) % 4];
            Vector3 prev = corners[(i + 3) % 4];

            // Calculate angle
            Vector3 dir1 = (next - current).normalized;
            Vector3 dir2 = (prev - current).normalized;
            float angle = Vector3.Angle(dir1, -dir2);
            angles.Add(angle);

            // Calculate side length
            sideLengths.Add(Vector3.Distance(current, next));
        }

        // Check if all angles are close to 90 degrees
        bool hasRightAngles = angles.All(angle => Mathf.Abs(angle - 90f) < angleTolerance);

        // Check if opposite sides are similar in length
        bool hasParallelSides = 
            Mathf.Abs(sideLengths[0] - sideLengths[2]) < distanceTolerance &&
            Mathf.Abs(sideLengths[1] - sideLengths[3]) < distanceTolerance;

        DebugLog($"Rectangle validation: Right angles: {hasRightAngles}, Parallel sides: {hasParallelSides}");
        return hasRightAngles && hasParallelSides;
    }

    public bool IsTriangle(List<Vector3> points)
    {
        if (points.Count < minPointsForShape)
        {
            DebugLog("Not enough points for triangle");
            return false;
        }

        // Simplify points to get main corners
        List<Vector3> corners = SimplifyPoints(points, 3);
        if (corners.Count != 3)
        {
            DebugLog("Could not find 3 corners for triangle");
            return false;
        }

        // Calculate angles
        float[] angles = new float[3];
        float[] sideLengths = new float[3];
        
        for (int i = 0; i < 3; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[(i + 1) % 3];
            Vector3 prev = corners[(i + 2) % 3];

            Vector3 dir1 = (next - current).normalized;
            Vector3 dir2 = (prev - current).normalized;
            angles[i] = Vector3.Angle(dir1, dir2);
            
            // Calculate side lengths
            sideLengths[i] = Vector3.Distance(current, next);
        }

        // Sum of angles in a triangle should be close to 180 degrees
        float angleSum = angles.Sum();
        bool validAngles = Mathf.Abs(angleSum - 180) < angleTolerance;

        // Check if any angle is too small or too large
        bool hasValidAngles = angles.All(angle => angle > 20 && angle < 150);

        // Check if the sides form a valid triangle (triangle inequality theorem)
        bool validSides = true;
        for (int i = 0; i < 3; i++)
        {
            float sum = sideLengths[(i + 1) % 3] + sideLengths[(i + 2) % 3];
            if (sum <= sideLengths[i])
            {
                validSides = false;
                break;
            }
        }

        DebugLog($"Triangle validation: Valid angles: {validAngles}, Valid sides: {validSides}, Angle sum: {angleSum}");
        return validAngles && hasValidAngles && validSides;
    }

    public bool IsCircle(List<Vector3> points)
    {
        if (points.Count < minPointsForShape)
        {
            DebugLog("Not enough points for circle");
            return false;
        }

        // Calculate center point
        Vector3 center = points.Aggregate(Vector3.zero, (sum, p) => sum + p) / points.Count;

        // Calculate average radius and variation
        float averageRadius = points.Average(p => Vector3.Distance(p, center));
        
        // Calculate radius variations
        float maxVariation = 0;
        float totalVariation = 0;
        
        foreach (var point in points)
        {
            float radius = Vector3.Distance(point, center);
            float variation = Mathf.Abs(radius - averageRadius) / averageRadius;
            maxVariation = Mathf.Max(maxVariation, variation);
            totalVariation += variation;
        }

        float averageVariation = totalVariation / points.Count;

        // Consider both max variation and average variation
        bool isCircular = maxVariation <= radiusTolerance && averageVariation <= (radiusTolerance * 0.5f);
        
        DebugLog($"Circle validation: Max variation: {maxVariation}, Avg variation: {averageVariation}");
        return isCircular;
    }

    private List<Vector3> SimplifyPoints(List<Vector3> points, int targetCount)
    {
        if (points.Count < targetCount) return points;

        List<Vector3> simplified = new List<Vector3>();
        float step = (float)points.Count / targetCount;
        
        for (int i = 0; i < targetCount; i++)
        {
            int index = Mathf.Min(Mathf.FloorToInt(i * step), points.Count - 1);
            simplified.Add(points[index]);
        }
        
        return simplified;
    }

    private void DebugLog(string message)
    {
        Debug.Log($"ShapeRecognizer: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"ShapeRecognizer: {message}");
        }
    }
}
