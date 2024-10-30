using UnityEngine;
using TMPro;

public class ShapeUIController : MonoBehaviour
{
    public TextMeshProUGUI shapeText;
    public TextMeshProUGUI debugText;
    private LineDrawer lineDrawer;

    private void Start()
    {
        lineDrawer = FindObjectOfType<LineDrawer>();
        if (lineDrawer != null)
        {
            lineDrawer.OnShapeDrawn += HandleShapeDrawn;
        }
    }

    private void OnDestroy()
    {
        if (lineDrawer != null)
        {
            lineDrawer.OnShapeDrawn -= HandleShapeDrawn;
        }
    }

    private void HandleShapeDrawn(ShapeDrawingEvent shapeEvent)
    {
        UpdateShapeUI(shapeEvent);
    }

    private void UpdateShapeUI(ShapeDrawingEvent shapeEvent)
    {
        string shapeTypeText = "Shape: " + shapeEvent.RecognizedShape.ToString();
        string debugInfo = $"Total Points: {shapeEvent.Points.Count}\n";
        
        if (shapeText != null)
            shapeText.text = shapeTypeText;
        if (debugText != null)
            debugText.text = debugInfo;
    }
}
