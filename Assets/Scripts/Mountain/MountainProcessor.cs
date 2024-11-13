using UnityEngine;

public class MountainProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private MountainGenerator mountainGenerator;

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        if (shapeEvent.RecognizedShape != ShapeType.Line)
            return;

        if (mountainGenerator.IsTerrainMode())
        {
            DebugLog("Processing line for terrain modification");
            mountainGenerator.GenerateTerrainModification(shapeEvent);
        }
        else if (mountainGenerator.IsMountainModeOn())
        {
            DebugLog("Processing line for mountain generation");
            mountainGenerator.GenerateMountain(shapeEvent);
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
}
