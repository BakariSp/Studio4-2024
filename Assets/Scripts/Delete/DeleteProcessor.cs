using UnityEngine;

public class DeleteProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private DeleteGenerator deleteGenerator;

    private void DebugLog(string message)
    {
        if (!deleteGenerator || !deleteGenerator.IsDeleteModeOn())
            return;
            
        Debug.Log($"DeleteProcessor: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"DeleteProcessor: {message}");
        }
    }

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        if (deleteGenerator == null || !deleteGenerator.IsDeleteModeOn())
        {
            return;
        }

        // Process the shape
        int deletedCount = deleteGenerator.ProcessDeleteArea(shapeEvent);
        DebugLog($"Deletion complete: {deletedCount} objects deleted");
    }
} 