using UnityEngine;

public class TreeProcessor : MonoBehaviour, IShapeProcessor
{
    [SerializeField] private TreeGenerator treeGenerator;

    private void DebugLog(string message)
    {
        if (!treeGenerator || !treeGenerator.IsTreeModeOn())
            return;
            
        Debug.Log($"TreeProcessor: {message}");
        if (DebugDisplay.Instance != null)
        {
            DebugDisplay.Instance.AddDebugMessage($"TreeProcessor: {message}");
        }
    }

    public void ProcessShape(ShapeDrawingEvent shapeEvent)
    {
        DebugLog($"Processing shape: {shapeEvent.RecognizedShape}, Tree Mode: {treeGenerator.IsTreeModeOn()}");

        if (treeGenerator == null)
        {
            DebugLog("TreeGenerator is not assigned!");
            return;
        }

        if (!treeGenerator.IsTreeModeOn())
        {
            DebugLog("Tree mode is OFF");
            return;
        }

        if (shapeEvent.RecognizedShape == ShapeType.Triangle)
        {
            DebugLog("Processing triangle for tree generation");
            GameObject tree = treeGenerator.GenerateTree(shapeEvent);
            DebugLog($"Tree generation result: {(tree != null ? "success" : "failed")}");
        }
        else
        {
            DebugLog($"Shape is not a triangle: {shapeEvent.RecognizedShape}");
        }
    }
}
