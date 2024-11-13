using UnityEngine;

public class WallProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private WallGenerator wallGenerator;

    private void DebugLog(string message)
    {
        Debug.Log($"WallProcessor: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"WallProcessor: {message}");
        }
    }

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        if (wallGenerator == null || !wallGenerator.IsWallModeOn())
        {
            return;
        }

        // In straight mode, only process if it's a line
        if (wallGenerator.IsStraightMode() && shapeEvent.IsShapeClosed)
        {
            DebugLog("Closed shapes not allowed in straight mode");
            return;
        }

        // Process the shape
        GameObject wall = wallGenerator.GenerateWall(shapeEvent);
        DebugLog($"Wall generation result: {(wall != null ? "success" : "failed")}");
    }
}
