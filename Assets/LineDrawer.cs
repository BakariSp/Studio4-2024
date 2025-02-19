using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI; // Add this for UI Image component
using System;

public class LineDrawer : MonoBehaviour
{
    public GameObject[] drawingObjects; // Array of drawing objects
    public GameObject lineRendererPrefab;
    public float distanceThreshold = 0.01f;
    // public GameObject leftHandPen;  // Left hand pen for rectangle mode
    // public GameObject rightHandPen; // Right hand pen for rectangle mode
    public bool isRectangleMode = false;
    public Material lineMaterial; // Add this line to specify a material for the line renderer
    public ShapeRecognizer shapeRecognizer;
    public Image displayImage; // Add this field for the UI Image component
    public int captureResolution = 512; // Resolution of the captured image
    private RenderTexture renderTexture;
    private Camera captureCamera;
    [SerializeField] private LayerMask captureLayerMask; // Add this to control what layers to capture

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

    public DoorSurfaceGenerator doorGenerator;
    public Camera userCamera; // Reference to the user's camera

    [Header("Door Detection Settings")]
    [Tooltip("Maximum angle (in degrees) between door normal and vertical axis")]
    [Range(0, 90)]
    public float maxDoorAngle = 30f; // Previously this was fixed at ~17 degrees (0.3 in dot product)

    [Tooltip("Minimum aspect ratio (height/width) for door detection")]
    public float minDoorAspectRatio = 1.2f;

    [Tooltip("Maximum aspect ratio (height/width) for door detection")]
    public float maxDoorAspectRatio = 3.0f;

    public bool InsideBoxCollider { get; set; }

    public event Action<ShapeDrawingEvent> OnShapeDrawn;
    private List<IShapeProcessor> shapeProcessors = new List<IShapeProcessor>();

    [Header("Shape Detection Settings")]
    [SerializeField] private float closeShapeThreshold = 0.1f; // Maximum distance between start and end points to consider a shape closed
    [SerializeField] private bool allowOpenShapes = false; // Whether to allow open shapes to be recognized

    [SerializeField] private DeleteControl deleteControl;
    [SerializeField] private bool isDeleteModeOn = false;
    private List<Vector3> currentDeletePoints = new List<Vector3>();

    void Start()
    {
        // Initialize capture camera
        GameObject captureCameraObj = new GameObject("CaptureCamera");
        captureCamera = captureCameraObj.AddComponent<Camera>();
        captureCamera.clearFlags = CameraClearFlags.SolidColor;
        captureCamera.backgroundColor = Color.clear;
        captureCamera.orthographic = true;
        captureCamera.enabled = false; // We'll only use it for capturing
        
        // Set the culling mask to ignore specific layers
        captureCamera.cullingMask = captureLayerMask;
        
        // Create render texture
        renderTexture = new RenderTexture(captureResolution, captureResolution, 24);
        renderTexture.antiAliasing = 4;

        // Only capture the "Drawing" layer
        captureCamera.cullingMask = (1 << LayerMask.NameToLayer("Drawing"));

        // Find and register all shape processors
        shapeProcessors.AddRange(GetComponents<IShapeProcessor>());
    }

    void Update()
    {
        /*
        if (isRectangleMode)
        {
            UpdateRectangleMode();
        }
        else
        {
            UpdateNormalMode();
        }
        */
        UpdateNormalMode();
    }

    private void UpdateNormalMode()
    {
        foreach (GameObject obj in drawingObjects)
        {
            if (obj.activeSelf)
            {
                if (!activeLines.ContainsKey(obj) || inactiveObjects.Contains(obj))
                {
                    if (!InsideBoxCollider || activeLines.ContainsKey(obj))
                    {
                        StartNewLine(obj);
                        inactiveObjects.Remove(obj);
                    }
                }
                if (activeLines.ContainsKey(obj))
                {
                    if (!InsideBoxCollider)
                    {
                        if (isDeleteModeOn)
                        {
                            HandleDeleteMode(obj);
                        }
                        else
                        {
                            DrawLine(obj);
                        }
                    }
                }
            }
            else if (!inactiveObjects.Contains(obj) && activeLines.ContainsKey(obj))
            {
                if (isDeleteModeOn)
                {
                    ProcessDeleteArea();
                }
                else
                {
                    // Line drawing just finished
                    LineRenderer finishedLine = activeLines[obj];
                    CheckShapeRecognition(finishedLine);
                }
                inactiveObjects.Add(obj);
            }
        }
    }

    /* Rectangle mode is disable for now
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
            GameObject rectangleObj = DrawRectangle(combinedPoints);
            if (rectangleObj != null)
            {
                CheckShapeRecognition(rectangleObj.GetComponent<LineRenderer>());
                CaptureDrawing(rectangleObj.GetComponent<LineRenderer>());
            }
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

    
    */
    // Rectangle mode is disable for now //

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

    private GameObject DrawRectangle(List<Vector3> points)
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
            return rectangleObj;
        }
        
        Debug.LogError("LineRenderer component not found on the instantiated prefab.");
        return null;
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
        if (points.Count < 4) return points;

        // Get the camera's view plane
        Plane viewPlane = new Plane(-userCamera.transform.forward, points[0]);
        Vector3 upDirection = Vector3.up;
        Vector3 rightDirection = Vector3.Cross(upDirection, -userCamera.transform.forward).normalized;

        // Project all points onto the view plane
        List<Vector3> projectedPoints = new List<Vector3>();
        foreach (Vector3 point in points)
        {
            Ray ray = new Ray(userCamera.transform.position, point - userCamera.transform.position);
            float enter;
            if (viewPlane.Raycast(ray, out enter))
            {
                projectedPoints.Add(ray.GetPoint(enter));
            }
        }

        if (projectedPoints.Count < 4) return points;

        // Calculate the center point
        Vector3 center = Vector3.zero;
        foreach (Vector3 point in projectedPoints)
        {
            center += point;
        }
        center /= projectedPoints.Count;

        // Find the principal axes in the view plane
        Vector3 primaryAxis = Vector3.zero;
        Vector3 secondaryAxis = Vector3.zero;
        float maxSpread = 0;

        // Project points onto the up and right directions to find the primary orientation
        foreach (Vector3 direction in new[] { upDirection, rightDirection })
        {
            float minProj = float.MaxValue;
            float maxProj = float.MinValue;
            foreach (Vector3 point in projectedPoints)
            {
                float proj = Vector3.Dot(point - center, direction);
                minProj = Mathf.Min(minProj, proj);
                maxProj = Mathf.Max(maxProj, proj);
            }
            float spread = maxProj - minProj;
            if (spread > maxSpread)
            {
                maxSpread = spread;
                primaryAxis = direction;
                secondaryAxis = Vector3.Cross(direction, -userCamera.transform.forward).normalized;
            }
        }

        // Calculate the rectangle bounds along the primary and secondary axes
        float minPrimary = float.MaxValue, maxPrimary = float.MinValue;
        float minSecondary = float.MaxValue, maxSecondary = float.MinValue;

        foreach (Vector3 point in projectedPoints)
        {
            Vector3 relativePos = point - center;
            float primaryProj = Vector3.Dot(relativePos, primaryAxis);
            float secondaryProj = Vector3.Dot(relativePos, secondaryAxis);
            
            minPrimary = Mathf.Min(minPrimary, primaryProj);
            maxPrimary = Mathf.Max(maxPrimary, primaryProj);
            minSecondary = Mathf.Min(minSecondary, secondaryProj);
            maxSecondary = Mathf.Max(maxSecondary, secondaryProj);
        }

        // Create the perfect rectangle corners
        List<Vector3> perfectCorners = new List<Vector3>
        {
            center + primaryAxis * minPrimary + secondaryAxis * minSecondary,
            center + primaryAxis * maxPrimary + secondaryAxis * minSecondary,
            center + primaryAxis * maxPrimary + secondaryAxis * maxSecondary,
            center + primaryAxis * minPrimary + secondaryAxis * maxSecondary,
            center + primaryAxis * minPrimary + secondaryAxis * minSecondary // Close the loop
        };

        return perfectCorners;
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

    public void StartNewLine(GameObject obj)
    {
        GameObject newLineObj = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
        // Set the layer to "Drawing"
        newLineObj.layer = LayerMask.NameToLayer("Drawing");
        
        LineRenderer newLineRenderer = newLineObj.GetComponent<LineRenderer>();
        if (newLineRenderer != null)
        {
            newLineRenderer.positionCount = 1;
            newLineRenderer.SetPosition(0, obj.transform.position);
            activeLines[obj] = newLineRenderer;
            allCreatedLines.Add(newLineObj);
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
        // Set the layer to "Drawing"
        tempLineObj.layer = LayerMask.NameToLayer("Drawing");
        
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

    private bool IsShapeClosed(Vector3[] points)
    {
        if (points.Length < 3) return false;
        float distanceBetweenEnds = Vector3.Distance(points[0], points[points.Length - 1]);
        return distanceBetweenEnds <= closeShapeThreshold;
    }

    private void CheckShapeRecognition(LineRenderer lineRenderer)
    {
        if (shapeRecognizer == null) return;

        Vector3[] points = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(points);
        
        bool isShapeClosed = IsShapeClosed(points);
        ShapeType recognizedShape = ShapeType.None;
        List<Vector3> perfectPoints = null;

        // Check for line when mountain mode is on
        MountainGenerator mountainGenerator = FindObjectOfType<MountainGenerator>();
        if (mountainGenerator != null && mountainGenerator.IsMountainModeOn() && !isShapeClosed)
        {
            recognizedShape = ShapeType.Line;
            DebugLog("Line detected for mountain generation");
        }

        // Only proceed with shape recognition if the shape is closed or we allow open shapes
        if (isShapeClosed || allowOpenShapes)
        {
            List<Vector3> processedPoints = points.ToList();
            
            // Only close the shape if it's actually closed
            if (isShapeClosed && points.Length > 2)
            {
                processedPoints.Add(processedPoints[0]);
            }

            DebugLog($"Shape Status: {(isShapeClosed ? "Closed" : "Open")}");
            DebugLog($"Points Count: {points.Length}");

            // For open shapes, only try to recognize certain shapes
            if (!isShapeClosed)
            {
                // Add specific open shape recognition here if needed
                DebugLog("Open shape detected - skipping closed shape recognition");
                recognizedShape = ShapeType.Line; // or other appropriate type
            }
            else
            {
                // Check for closed shapes
                if (shapeRecognizer.IsRectangle(processedPoints))
                {
                    recognizedShape = ShapeType.Rectangle;
                    perfectPoints = CreatePerfectRectangle(processedPoints);
                    DebugLog("Shape recognized as Rectangle");
                }
                else if (shapeRecognizer.IsCircle(processedPoints))
                {
                    recognizedShape = ShapeType.Circle;
                    DebugLog("Shape recognized as Circle");
                }
                else if (shapeRecognizer.IsTriangle(processedPoints))
                {
                    recognizedShape = ShapeType.Triangle;
                    DebugLog("Shape recognized as Triangle");
                }
                else
                {
                    DebugLog("No shape recognized");
                }
            }

            // Update the line renderer with perfect points if available
            if (perfectPoints != null)
            {
                lineRenderer.positionCount = perfectPoints.Count;
                lineRenderer.SetPositions(perfectPoints.ToArray());
            }
            
            // Create and broadcast the shape event
            var shapeEvent = new ShapeDrawingEvent(
                lineRenderer, 
                processedPoints, 
                recognizedShape,
                isShapeClosed  // Add this property to ShapeDrawingEvent
            );
            
            // Notify all processors
            foreach (var processor in shapeProcessors)
            {
                processor.ProcessShape(shapeEvent);
            }
            
            OnShapeDrawn?.Invoke(shapeEvent);
            CaptureDrawing(lineRenderer);
        }
        else
        {
            DebugLog("Shape is not closed and open shapes are not allowed");
        }
    }

    private void DebugLog(string message)
    {
        Debug.Log(message);
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage(message);
        }
    }

    private void CaptureDrawing(LineRenderer lineRenderer)
    {
        if (displayImage == null || lineRenderer == null) return;

        // Calculate bounds of just this line
        Bounds bounds = CalculateLineBounds(lineRenderer);
        
        // Position and setup camera
        captureCamera.orthographicSize = bounds.extents.magnitude;
        captureCamera.transform.position = bounds.center - Vector3.forward * 10;
        captureCamera.targetTexture = renderTexture;
        
        // Temporarily disable all other line renderers
        Dictionary<LineRenderer, bool> previousStates = new Dictionary<LineRenderer, bool>();
        foreach (GameObject lineObj in allCreatedLines)
        {
            if (lineObj != null)
            {
                LineRenderer line = lineObj.GetComponent<LineRenderer>();
                if (line != null && line != lineRenderer)
                {
                    previousStates[line] = line.enabled;
                    line.enabled = false;
                }
            }
        }
        
        // Render to texture
        captureCamera.Render();
        
        // Create a new texture and read pixels
        Texture2D texture = new Texture2D(captureResolution, captureResolution, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, captureResolution, captureResolution), 0, 0);
        texture.Apply();
        
        // Create sprite and assign to image
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        displayImage.sprite = sprite;
        
        // Reset render texture
        RenderTexture.active = null;
        
        // Restore previous line renderer states
        foreach (var kvp in previousStates)
        {
            if (kvp.Key != null)
            {
                kvp.Key.enabled = kvp.Value;
            }
        }
    }

    private Bounds CalculateLineBounds(LineRenderer lineRenderer)
    {
        Bounds bounds = new Bounds();
        
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(positions);
        
        if (positions.Length > 0)
        {
            bounds = new Bounds(positions[0], Vector3.zero);
            for (int i = 1; i < positions.Length; i++)
            {
                bounds.Encapsulate(positions[i]);
            }
        }
        
        // Add some padding
        bounds.Expand(bounds.size * 0.1f);
        return bounds;
    }

    void OnDestroy()
    {
        // Clean up
        if (renderTexture != null)
            renderTexture.Release();
        if (captureCamera != null)
            Destroy(captureCamera.gameObject);
    }

    private void HandleDeleteMode(GameObject obj)
    {
        if (!isDeleteModeOn) return;

        Vector3 currentPosition = obj.transform.position;
        currentDeletePoints.Add(currentPosition);
    }

    private void ProcessDeleteArea()
    {
        if (!isDeleteModeOn || currentDeletePoints.Count < 2) return;

        ShapeDrawingEvent deleteEvent = new ShapeDrawingEvent(
            null,
            currentDeletePoints,
            ShapeType.Line,
            false
        );
        
        deleteControl.ProcessDeleteArea(deleteEvent);
        currentDeletePoints.Clear();
    }

    public void SetDeleteMode(bool state)
    {
        isDeleteModeOn = state;
        if (!state)
        {
            currentDeletePoints.Clear();
        }
    }

    public void ToggleDeleteMode()
    {
        isDeleteModeOn = !isDeleteModeOn;
    }

    public bool IsDeleteModeOn()
    {
        return isDeleteModeOn;
    }
}
