using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DrawLine : MonoBehaviour
{
    public Transform drawingObject;
    private LineRenderer lineRenderer;
    private Vector3 lastPosition;
    private float distanceThreshold = 0.01f;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (drawingObject != null)
        {
            lastPosition = drawingObject.position;
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, lastPosition);
        }
    }

    // Basical function
    void Update()
    {
        if (drawingObject != null && Vector3.Distance(drawingObject.position, lastPosition) > distanceThreshold)
        {
            Vector3 currentPosition = drawingObject.position;
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPosition);

            float slope = SlopeCalculator.CalculateSlope(lastPosition, currentPosition);
            UpdateLineColor(slope);
            
            lastPosition = currentPosition;
        }
    }


    // updated function
    /*
    void Update()
    {
        if (drawingObject != null && Vector3.Distance(drawingObject.position, lastPosition) > distanceThreshold)
        {
            Vector3 currentPosition = drawingObject.position;

            // Interpolate points between lastPosition and currentPosition
            InterpolateLine(lastPosition, currentPosition);

            lastPosition = currentPosition;
        }
    }

    private void InterpolateLine(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        int pointsToInterpolate = Mathf.CeilToInt(distance / distanceThreshold); // Calculate how many points to interpolate based on the distance and threshold

        for (int i = 1; i <= pointsToInterpolate; i++)
        {
            float t = i / (float)pointsToInterpolate;
            Vector3 interpolatedPoint = Vector3.Lerp(start, end, t);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, interpolatedPoint);

            float slope = SlopeCalculator.CalculateSlope(new Vector2(start.x, start.y), new Vector2(interpolatedPoint.x, interpolatedPoint.y));
            UpdateLineColor(slope);
        }
    }
    */


    private void UpdateLineColor(float slope)
    {
        Debug.Log(slope);
        Color lineColor = SlopeCalculator.GetColorFromSlope(slope);

        // Assuming the LineRenderer is using a material with a shader that has a "_Color" property
        lineRenderer.material.SetColor("_Color", lineColor);
    }

}
