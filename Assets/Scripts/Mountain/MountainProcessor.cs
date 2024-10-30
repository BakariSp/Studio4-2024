using UnityEngine;

public class MountainProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private MountainGenerator mountainGenerator;

    private void DebugLog(string message)
    {
        Debug.Log(message);
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage(message);
        }
    }

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        if (!mountainGenerator.IsMountainModeOn())
            return;

        if (shapeEvent.RecognizedShape == ShapeType.Line)
        {
            DebugLog("Processing line for mountain generation");
            mountainGenerator.GenerateMountain(shapeEvent);
        }
    }
}
