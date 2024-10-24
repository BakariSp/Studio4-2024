using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LineDrawer : MonoBehaviour
{
    public GameObject[] drawingObjects; // Array of drawing objects
    public GameObject lineRendererPrefab;
    public float distanceThreshold = 0.01f;
    public GameObject leftHandPen;  // Left hand pen for rectangle mode
    public GameObject rightHandPen; // Right hand pen for rectangle mode
    public bool isRectangleMode = false;
    public Material lineMaterial; // Add this line to specify a material for the line renderer

    public enum RectangleMode
    {
        Raw,
        Smoothed,
        Perfect
    }

    public RectangleMode currentRectangleMode = RectangleMode.Raw;
    public float smoothingFactor = 0.5f; // 0 = no smoothing, 1 = max smoothing

    private Dictionary<GameObject, LineRenderer> activeLines = new Dictionary<GameObject, LineRenderer>();
    private HashSet<GameObject> inactiveObjects = new HashSet<GameObject>();
    private List<Vector3> leftHandPoints = new List<Vector3>();
    private List<Vector3> rightHandPoints = new List<Vector3>();
    private bool isDrawingRectangle = false;
    private bool canStartNewRectangle = true;
    private LineRenderer leftHandLine;
    private LineRenderer rightHandLine;
    private List<GameObject> allCreatedLines = new List<GameObject>();

    void Update()
    {
        if (isRectangleMode)
        {
            UpdateRectangleMode();
        }
        else
        {
            UpdateNormalMode();
        }
    }

    private void UpdateNormalMode()
    {
        foreach (GameObject obj in drawingObjects)
        {
            if (obj.activeSelf)
            {
                if (!activeLines.ContainsKey(obj) || inactiveObjects.Contains(obj))
                {
                    StartNewLine(obj);
                    inactiveObjects.Remove(obj);
                }
                DrawLine(obj);
            }
            else
            {
                inactiveObjects.Add(obj);
            }
        }
    }

    private void UpdateRectangleMode()
    {
        bool bothPensActive = leftHandPen.activeSelf && rightHandPen.activeSelf;

        if (bothPensActive && canStartNewRectangle)
        {
            StartRectangleDrawing();
        }
        else if (bothPensActive && isDrawingRectangle)
        {
            ContinueRectangleDrawing();
        }
        else if (!bothPensActive && isDrawingRectangle)
        {
            FinishRectangleDrawing();
        }
        else if (!bothPensActive)
        {
            canStartNewRectangle = true;
        }
    }

    private void StartRectangleDrawing()
    {
        isDrawingRectangle = true;
        canStartNewRectangle = false;
        leftHandPoints.Clear();
        rightHandPoints.Clear();

        // Create temporary line renderers for drawing
        leftHandLine = CreateTemporaryLineRenderer();
        rightHandLine = CreateTemporaryLineRenderer();

        Debug.Log("Started new rectangle drawing");
    }

    private void ContinueRectangleDrawing()
    {
        Vector3 leftHandPosition = leftHandPen.transform.position;
        Vector3 rightHandPosition = rightHandPen.transform.position;

        leftHandPoints.Add(leftHandPosition);
        rightHandPoints.Add(rightHandPosition);

        // Update temporary line renderers
        UpdateTemporaryLineRenderer(leftHandLine, leftHandPoints);
        UpdateTemporaryLineRenderer(rightHandLine, rightHandPoints);
    }

    private void FinishRectangleDrawing()
    {
        if (leftHandPoints.Count > 0 && rightHandPoints.Count > 0)
        {
            List<Vector3> combinedPoints = CombineAndSmoothPoints();
            DrawRectangle(combinedPoints);
            Debug.Log($"Rectangle drawing finished. Total points: {combinedPoints.Count}");
        }
        else
        {
            Debug.Log("Rectangle drawing cancelled - not enough points");
        }

        // Destroy temporary line renderers
        Destroy(leftHandLine.gameObject);
        Destroy(rightHandLine.gameObject);

        isDrawingRectangle = false;
        leftHandPoints.Clear();
        rightHandPoints.Clear();
    }

    private List<Vector3> CombineAndSmoothPoints()
    {
        List<Vector3> combinedPoints = new List<Vector3>();
        combinedPoints.AddRange(leftHandPoints);
        combinedPoints.AddRange(rightHandPoints.AsEnumerable().Reverse());
        combinedPoints.Add(combinedPoints[0]); // Close the shape

        // Simple smoothing by averaging nearby points
        List<Vector3> smoothedPoints = new List<Vector3>();
        int windowSize = 5;
        for (int i = 0; i < combinedPoints.Count; i++)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            for (int j = -windowSize / 2; j <= windowSize / 2; j++)
            {
                int index = (i + j + combinedPoints.Count) % combinedPoints.Count;
                sum += combinedPoints[index];
                count++;
            }
            smoothedPoints.Add(sum / count);
        }

        return smoothedPoints;
    }

    private void DrawRectangle(List<Vector3> points)
    {
        List<Vector3> finalPoints;

        switch (currentRectangleMode)
        {
            case RectangleMode.Smoothed:
                finalPoints = SmoothPoints(points);
                break;
            case RectangleMode.Perfect:
                finalPoints = CreatePerfectRectangle(points);
                break;
            default: // RectangleMode.Raw
                finalPoints = points;
                break;
        }

        GameObject rectangleObj = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer rectangleRenderer = rectangleObj.GetComponent<LineRenderer>();
        if (rectangleRenderer != null)
        {
            rectangleRenderer.positionCount = finalPoints.Count;
            rectangleRenderer.SetPositions(finalPoints.ToArray());
            rectangleRenderer.loop = true;
            
            // Set material and color
            rectangleRenderer.material = lineMaterial;
            rectangleRenderer.startColor = Color.white;
            rectangleRenderer.endColor = Color.white;
            
            // Set width
            rectangleRenderer.startWidth = 0.01f;
            rectangleRenderer.endWidth = 0.01f;

            allCreatedLines.Add(rectangleObj); // Add to the list of all created lines

            Debug.Log($"Rectangle drawn with {finalPoints.Count} points. Mode: {currentRectangleMode}");
        }
        else
        {
            Debug.LogError("LineRenderer component not found on the instantiated prefab.");
        }
    }

    private List<Vector3> SmoothPoints(List<Vector3> points)
    {
        List<Vector3> smoothedPoints = new List<Vector3>();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            Vector3 nextPoint = points[(i + 1) % points.Count];
            Vector3 smoothedPoint = Vector3.Lerp(point, nextPoint, smoothingFactor);
            smoothedPoints.Add(smoothedPoint);
        }
        return smoothedPoints;
    }

    private List<Vector3> CreatePerfectRectangle(List<Vector3> points)
    {
        if (points.Count < 2)
            return points;

        // Sample points if there are too many
        List<Vector3> sampledPoints = SamplePoints(points, 100); // Sample up to 100 points

        // Calculate the center point
        Vector3 center = sampledPoints.Aggregate(Vector3.zero, (sum, p) => sum + p) / sampledPoints.Count;

        // Calculate the covariance matrix
        float[,] covariance = new float[3, 3];
        foreach (Vector3 point in sampledPoints)
        {
            Vector3 diff = point - center;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    covariance[i, j] += diff[i] * diff[j];
        }
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                covariance[i, j] /= sampledPoints.Count;

        // Find the two principal axes (largest eigenvectors)
        Vector3 axis1 = GetPrincipalAxis(covariance);
        Vector3 axis2 = GetSecondPrincipalAxis(covariance, axis1);

        // Project points onto the principal axes
        float minProj1 = float.MaxValue, maxProj1 = float.MinValue;
        float minProj2 = float.MaxValue, maxProj2 = float.MinValue;
        foreach (Vector3 point in points)
        {
            Vector3 diff = point - center;
            float proj1 = Vector3.Dot(diff, axis1);
            float proj2 = Vector3.Dot(diff, axis2);
            minProj1 = Mathf.Min(minProj1, proj1);
            maxProj1 = Mathf.Max(maxProj1, proj1);
            minProj2 = Mathf.Min(minProj2, proj2);
            maxProj2 = Mathf.Max(maxProj2, proj2);
        }

        // Create the rectangle corners
        Vector3 corner1 = center + axis1 * minProj1 + axis2 * minProj2;
        Vector3 corner2 = center + axis1 * maxProj1 + axis2 * minProj2;
        Vector3 corner3 = center + axis1 * maxProj1 + axis2 * maxProj2;
        Vector3 corner4 = center + axis1 * minProj1 + axis2 * maxProj2;

        return new List<Vector3> { corner1, corner2, corner3, corner4, corner1 };
    }

    private List<Vector3> SamplePoints(List<Vector3> points, int sampleSize)
    {
        if (points.Count <= sampleSize)
            return new List<Vector3>(points);

        List<Vector3> sampledPoints = new List<Vector3>();
        System.Random random = new System.Random();

        for (int i = 0; i < sampleSize; i++)
        {
            int index = random.Next(points.Count);
            sampledPoints.Add(points[index]);
        }

        return sampledPoints;
    }

    private Vector3 GetPrincipalAxis(float[,] covariance)
    {
        // Simple power iteration to find the largest eigenvector
        Vector3 v = new Vector3(1, 1, 1).normalized;
        for (int i = 0; i < 20; i++) // Increased iterations for better convergence
        {
            Vector3 Av = MultiplyCovariance(covariance, v);
            v = Av.normalized;
        }
        return v;
    }

    private Vector3 GetSecondPrincipalAxis(float[,] covariance, Vector3 firstAxis)
    {
        // Find the second largest eigenvector (orthogonal to the first)
        Vector3 v = Vector3.Cross(firstAxis, new Vector3(0, 1, 0)).normalized;
        if (v.magnitude < 0.01f) // If first axis is close to (0,1,0), use a different vector
            v = Vector3.Cross(firstAxis, new Vector3(1, 0, 0)).normalized;

        for (int i = 0; i < 20; i++)
        {
            Vector3 Av = MultiplyCovariance(covariance, v);
            v = (Av - Vector3.Dot(Av, firstAxis) * firstAxis).normalized;
        }
        return v;
    }

    private Vector3 MultiplyCovariance(float[,] covariance, Vector3 v)
    {
        return new Vector3(
            covariance[0, 0] * v.x + covariance[0, 1] * v.y + covariance[0, 2] * v.z,
            covariance[1, 0] * v.x + covariance[1, 1] * v.y + covariance[1, 2] * v.z,
            covariance[2, 0] * v.x + covariance[2, 1] * v.y + covariance[2, 2] * v.z
        );
    }

    private void StartNewLine(GameObject obj)
    {
        GameObject newLineObj = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer newLineRenderer = newLineObj.GetComponent<LineRenderer>();
        if (newLineRenderer != null)
        {
            newLineRenderer.positionCount = 1;
            newLineRenderer.SetPosition(0, obj.transform.position);
            activeLines[obj] = newLineRenderer;
            allCreatedLines.Add(newLineObj); // Add to the list of all created lines
        }
    }

    private void DrawLine(GameObject obj)
    {
        LineRenderer lineRenderer = activeLines[obj];
        Vector3 currentPosition = obj.transform.position;

        if (lineRenderer.positionCount == 0 || Vector3.Distance(currentPosition, lineRenderer.GetPosition(lineRenderer.positionCount - 1)) > distanceThreshold)
        {
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPosition);
        }
    }

    private void RemoveLine(GameObject obj)
    {
        if (activeLines.TryGetValue(obj, out LineRenderer lineRenderer))
        {
            Destroy(lineRenderer.gameObject);
            activeLines.Remove(obj);
        }
    }

    public void ToggleRectangleMode(bool isRectangle)
    {
        isRectangleMode = isRectangle;
    }

    private LineRenderer CreateTemporaryLineRenderer()
    {
        GameObject tempLineObj = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer tempLine = tempLineObj.GetComponent<LineRenderer>();
        if (tempLine != null)
        {
            tempLine.positionCount = 0;
            tempLine.material = lineMaterial;
            tempLine.startColor = Color.gray; // Use a different color for temporary lines
            tempLine.endColor = Color.gray;
            tempLine.startWidth = 0.005f; // Thinner line for temporary drawing
            tempLine.endWidth = 0.005f;
        }
        return tempLine;
    }

    private void UpdateTemporaryLineRenderer(LineRenderer lineRenderer, List<Vector3> points)
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    public void CleanAllLines()
    {
        foreach (GameObject lineObj in allCreatedLines)
        {
            Destroy(lineObj);
        }

        allCreatedLines.Clear();
        activeLines.Clear();
        inactiveObjects.Clear();

        Debug.Log("All lines have been cleaned.");
    }
}
