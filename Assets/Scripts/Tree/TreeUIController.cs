using UnityEngine;
using TMPro;

public class TreeUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI treeModeText;
    [SerializeField] private TextMeshProUGUI treeDebugText;
    private TreeGenerator treeGenerator;

    private void Start()
    {
        treeGenerator = FindObjectOfType<TreeGenerator>();
        if (treeGenerator == null)
        {
            Debug.LogError("TreeUIController: TreeGenerator not found in scene");
            return;
        }

        // Verify UI components
        if (treeModeText == null)
            Debug.LogError("TreeUIController: Tree Mode Text is not assigned");
        if (treeDebugText == null)
            Debug.LogError("TreeUIController: Tree Debug Text is not assigned");

        // Initial UI update
        UpdateTreeModeUI(treeGenerator.IsTreeModeOn());
        UpdateTreeDebugInfo("Tree UI Controller initialized");
    }

    public void UpdateTreeModeUI(bool isTreeModeOn)
    {
        if (treeModeText != null)
        {
            treeModeText.text = $"Tree Mode: {(isTreeModeOn ? "ON" : "OFF")}";
            Debug.Log($"TreeUIController: Updated mode UI - {treeModeText.text}");
        }
    }

    public void UpdateTreeDebugInfo(string debugInfo)
    {
        if (treeDebugText != null)
        {
            treeDebugText.text = debugInfo;
            Debug.Log($"TreeUIController: Updated debug info - {debugInfo}");
        }
    }
} 