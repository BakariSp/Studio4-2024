using UnityEngine;
using System.Collections.Generic;

public class ShapeDrawingEvent
{
    public LineRenderer LineRenderer { get; private set; }
    public List<Vector3> Points { get; private set; }
    public ShapeType RecognizedShape { get; private set; }
    public bool IsShapeClosed { get; private set; }
    public float MountainHeight { get; private set; }

    public ShapeDrawingEvent(
        LineRenderer lineRenderer, 
        List<Vector3> points, 
        ShapeType recognizedShape,
        bool isShapeClosed,
        float mountainHeight = 50f)
    {
        LineRenderer = lineRenderer;
        Points = points;
        RecognizedShape = recognizedShape;
        IsShapeClosed = isShapeClosed;
        MountainHeight = mountainHeight;
    }
}

public enum ShapeType
{
    None,
    Line,
    Rectangle,
    Circle,
    Triangle,
    Door,
    Mountain
} 