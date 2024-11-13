using UnityEngine;
using TMPro;

public class GeneratorUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI modeStatusText;
    [SerializeField] private TextMeshProUGUI debugInfoText;

    private static GeneratorUIController instance;
    public static GeneratorUIController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GeneratorUIController>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Verify UI components
        if (modeStatusText == null)
            Debug.LogError("GeneratorUIController: Mode Status Text is not assigned");
        if (debugInfoText == null)
            Debug.LogError("GeneratorUIController: Debug Info Text is not assigned");

        UpdateModeStatus("No Mode Active");
        UpdateDebugInfo("Generator UI Controller initialized");
    }

    public void UpdateModeStatus(string status)
    {
        if (modeStatusText != null)
        {
            modeStatusText.text = status;
            Debug.Log($"GeneratorUIController: Updated mode status - {status}");
        }
    }

    public void UpdateDebugInfo(string debugInfo)
    {
        if (debugInfoText != null)
        {
            debugInfoText.text = debugInfo;
            Debug.Log($"GeneratorUIController: Updated debug info - {debugInfo}");
        }
    }
} 