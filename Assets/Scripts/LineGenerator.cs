using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer), typeof(Light))]
public class LineGenerator : MonoBehaviour
{
    public Transform drawingObject1; // First drawing object
    public Transform drawingObject2; // Second drawing object
    private LineRenderer lineRenderer;
    public GameObject lineRendererPrefab;
    private SoundGenerator soundGenerator;
    private PrefabGenerator prefabGenerator;
    private Vector3 lastPosition;
    public Light drawingLight; // Reference to the Light component
    private float distanceThreshold = 0.01f;
    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private List<LineRenderer> lineRenderers = new List<LineRenderer>(); 
    private int historySize = 5; // Adjustable based on desired smoothness

    private float proximityThreshold = 0.1f; // Threshold for how close the objects need to be to enable drawing
    private bool isDrawingEnabled = false; // Controls whether drawing is currently enabled

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        drawingLight = GetComponent<Light>(); // Get the Light component
        soundGenerator = GetComponent<SoundGenerator>();
        prefabGenerator = GetComponent<PrefabGenerator>();

        if (drawingObject1 != null)
        {
            lastPosition = drawingObject1.position;
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, lastPosition);
        }
    }

    void Update()
    {
        // if (drawingObject != null && Vector3.Distance(drawingObject.position, lastPosition) > distanceThreshold)
        // {
        //     Vector3 currentPosition = drawingObject.position;
        //     InterpolateLine(lastPosition, currentPosition);
        //     lastPosition = currentPosition;
        // }

        //  if (drawingObject != null && Vector3.Distance(drawingObject.position, lastPosition) > distanceThreshold)
        // {
        //     Vector3 currentPosition = drawingObject.position;
        //     lineRenderer.positionCount++;
        //     lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPosition);

        //     float slope = SlopeCalculator.CalculateSlope(lastPosition, currentPosition);
        //     Color lightColor = SlopeCalculator.GetColorFromSlope(slope);
        //     UpdateLightColor(lightColor);
            
        //     soundGenerator.TriggerSoundBasedOnSlope(slope);
        //     prefabGenerator.GeneratePrefabForSlope(slope);
        //     lastPosition = currentPosition;
        // }

        
    
        // if (drawingObject != null)
        // {
        //     Vector3 currentPosition = drawingObject.position;
            
        //     // Enqueue the current position and ensure the queue doesn't exceed 'historySize'
        //     positionHistory.Enqueue(currentPosition);
        //     while (positionHistory.Count > historySize)
        //     {
        //         positionHistory.Dequeue();
        //     }

        //     // Calculate the averaged position from the history
        //     Vector3 averagedPosition = Vector3.zero;
        //     foreach (Vector3 pos in positionHistory)
        //     {
        //         averagedPosition += pos;
        //     }
        //     averagedPosition /= positionHistory.Count;

        //     // Use 'averagedPosition' instead of 'currentPosition' for drawing and calculations
        //     if (Vector3.Distance(averagedPosition, lastPosition) > distanceThreshold)
        //     {
        //         lineRenderer.positionCount++;
        //         lineRenderer.SetPosition(lineRenderer.positionCount - 1, averagedPosition);

        //         float slope = SlopeCalculator.CalculateSlope(lastPosition, averagedPosition);
        //         Color lightColor = SlopeCalculator.GetColorFromSlope(slope);
        //         UpdateLightColor(lightColor);

        //         soundGenerator.TriggerSoundBasedOnSlope(slope);
        //         prefabGenerator.GeneratePrefabForSlope(slope);

        //         lastPosition = averagedPosition; // Update 'lastPosition' with 'averagedPosition'
        //     }
        // }


        // Check if both drawing objects are assigned
        /* 
        if (drawingObject1 != null && drawingObject2 != null)
        {
            // Check the distance between the two drawing objects
            if (Vector3.Distance(drawingObject1.position, drawingObject2.position) <= proximityThreshold)
            {
                if (!isDrawingEnabled)
                {
                    // Enable drawing if the objects are close enough and drawing was previously disabled
                    isDrawingEnabled = true;
                    lastPosition = drawingObject1.position; // Update last position to current for smooth continuation
                }
            }
            else
            {
                // Disable drawing if the objects are too far apart
                isDrawingEnabled = false;
            }

            // Proceed with drawing if enabled
            if (isDrawingEnabled)
            {
                Vector3 currentPosition = drawingObject1.position; // Use the first object for drawing

                // Determine if the current position is far enough from the last position to warrant an update
                if (Vector3.Distance(currentPosition, lastPosition) > distanceThreshold)
                {
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPosition);

                    float slope = SlopeCalculator.CalculateSlope(lastPosition, currentPosition);
                    Color lightColor = SlopeCalculator.GetColorFromSlope(slope);
                    UpdateLightColor(lightColor);

                    soundGenerator.TriggerSoundBasedOnSlope(slope);
                    prefabGenerator.GeneratePrefabForSlope(slope);

                    lastPosition = currentPosition; // Update lastPosition for next frame's comparison
                }
            }
        }
        */

        if (drawingObject1 != null && drawingObject2 != null)
        {
            if (Vector3.Distance(drawingObject1.position, drawingObject2.position) <= proximityThreshold)
            {
                if (!isDrawingEnabled)
                {
                    isDrawingEnabled = true;
                    StartNewLine(drawingObject1.position);
                }
            }
            else
            {
                if (isDrawingEnabled)
                {
                    isDrawingEnabled = false;
                    // Actions upon disabling drawing could be added here
                }
            }

            if (isDrawingEnabled && lineRenderers.Count > 0)
            {
                DrawLine(drawingObject1.position);
            }
        }
    }


    private void StartNewLine(Vector3 startPosition)
    {
        GameObject newLineObj = Instantiate(lineRendererPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer newLineRenderer = newLineObj.GetComponent<LineRenderer>();
        if (newLineRenderer != null)
        {
            newLineRenderer.positionCount = 1;
            newLineRenderer.SetPosition(0, startPosition);
            lineRenderers.Add(newLineRenderer);
        }
    }

    private void DrawLine(Vector3 currentPosition)
    {
        LineRenderer currentLineRenderer = lineRenderers[lineRenderers.Count - 1];
        if (currentLineRenderer.positionCount == 0 || Vector3.Distance(currentPosition, currentLineRenderer.GetPosition(currentLineRenderer.positionCount - 1)) > distanceThreshold)
        {
            currentLineRenderer.positionCount++;
            currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, currentPosition);

            float slope = SlopeCalculator.CalculateSlope(currentLineRenderer.GetPosition(currentLineRenderer.positionCount - 2), currentPosition);
            Color lightColor = SlopeCalculator.GetColorFromSlope(slope);
            UpdateLightColor(lightColor);

            soundGenerator.TriggerSoundBasedOnSlope(slope);
            prefabGenerator.GeneratePrefabForSlope(slope);
        }
    }

        
    

    private void InterpolateLine(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int pointsToInterpolate = Mathf.CeilToInt(distance / distanceThreshold);
        float slope = 0f;

        for (int i = 1; i <= pointsToInterpolate; i++)
        {
            float t = i / (float)pointsToInterpolate;
            Vector3 interpolatedPoint = Vector3.Lerp(start, end, t);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, interpolatedPoint);

            // Calculate slope for color
            slope = SlopeCalculator.CalculateSlope(new Vector2(start.x, start.y), new Vector2(interpolatedPoint.x, interpolatedPoint.y));
            Color lightColor = SlopeCalculator.GetColorFromSlope(slope);
            UpdateLightColor(lightColor); // Update the light color based on the slope
        }
        
        soundGenerator.TriggerSoundBasedOnSlope(slope);

    }

    private void UpdateLightColor(Color color)
    {
        drawingLight.color = color;
    }
}

