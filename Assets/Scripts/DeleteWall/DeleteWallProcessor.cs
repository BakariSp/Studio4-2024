using UnityEngine;

public class DeleteWallProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private DeleteWallGenerator deleteWallGenerator;

    private void DebugLog(string message)
    {
        if (!deleteWallGenerator || !deleteWallGenerator.IsDeleteModeOn())
            return;
            
        Debug.Log($"WallProcessor: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"WallProcessor: {message}");
        }
    }

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        if (deleteWallGenerator == null || !deleteWallGenerator.IsDeleteModeOn())
        {
            return;
        }

        // In straight mode, only process if it's a line
        if (deleteWallGenerator.IsStraightMode() && shapeEvent.IsShapeClosed)
        {
            DebugLog("Closed shapes not allowed in straight mode");
            return;
        }

        // Process the shape
        GameObject wall = deleteWallGenerator.GenerateWall(shapeEvent);
        DebugLog($"Wall generation result: {(wall != null ? "success" : "failed")}");
    }
}
