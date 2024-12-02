using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildModeManager : MonoBehaviour
{
    [SerializeField] private TreeGenerator treeGenerator;
    [SerializeField] private MountainGenerator mountainGenerator;
    [SerializeField] private WallGenerator wallGenerator;
    
    [Header("UI Toggles")]
    [SerializeField] private Toggle treeToggle;
    [SerializeField] private Toggle mountainToggle;
    [SerializeField] private Toggle wallToggle;

    private bool isUpdatingToggles = false;

    private void Start()
    {
        // Ensure all modes are off at start
        DisableAllModes();
        
        // Add listeners to toggles
        if (treeToggle != null)
            treeToggle.onValueChanged.AddListener(OnTreeToggleChanged);
        if (mountainToggle != null)
            mountainToggle.onValueChanged.AddListener(OnMountainToggleChanged);
        if (wallToggle != null)
            wallToggle.onValueChanged.AddListener(OnWallToggleChanged);
    }

    public void ToggleTreeMode()
    {
        if (treeGenerator.IsTreeModeOn())
        {
            DisableAllModes();
        }
        else
        {
            DisableAllModes();
            treeGenerator.SetTreeMode(true);
            UpdateUIStatus("Tree Mode");
        }
        UpdateToggles();
    }

    public void ToggleMountainMode()
    {
        if (mountainGenerator.IsMountainModeOn())
        {
            DisableAllModes();
        }
        else
        {
            DisableAllModes();
            mountainGenerator.SetMountainMode(true);
            UpdateUIStatus("Mountain Mode");
        }
        UpdateToggles();
    }

    public void ToggleWallMode()
    {
        if (wallGenerator.IsWallModeOn())
        {
            DisableAllModes();
        }
        else
        {
            DisableAllModes();
            wallGenerator.SetWallMode(true);
            UpdateUIStatus("Wall Mode");
        }
        UpdateToggles();
    }

    private void DisableAllModes()
    {
        if (treeGenerator != null)
            treeGenerator.SetTreeMode(false);
        
        if (mountainGenerator != null)
        {
            mountainGenerator.SetMountainMode(false);
            mountainGenerator.SetTerrainMode(false);
        }
        
        if (wallGenerator != null)
            wallGenerator.SetWallMode(false);

        UpdateUIStatus("No Mode Active");
    }

    private void UpdateUIStatus(string status)
    {
        if (GeneratorUIController.Instance != null)
        {
            GeneratorUIController.Instance.UpdateModeStatus(status);
        }
    }

    // Helper methods to check current active mode
    public bool IsTreeModeActive() => treeGenerator != null && treeGenerator.IsTreeModeOn();
    public bool IsMountainModeActive() => mountainGenerator != null && mountainGenerator.IsMountainModeOn();
    public bool IsWallModeActive() => wallGenerator != null && wallGenerator.IsWallModeOn();

    private void UpdateToggles()
    {
        if (isUpdatingToggles) return; // Prevent recursive calls
        
        isUpdatingToggles = true;
        
        if (treeToggle != null)
            treeToggle.SetIsOnWithoutNotify(treeGenerator != null && treeGenerator.IsTreeModeOn());
        
        if (mountainToggle != null)
            mountainToggle.SetIsOnWithoutNotify(mountainGenerator != null && mountainGenerator.IsMountainModeOn());
        
        if (wallToggle != null)
            wallToggle.SetIsOnWithoutNotify(wallGenerator != null && wallGenerator.IsWallModeOn());
            
        isUpdatingToggles = false;
    }

    private void OnTreeToggleChanged(bool isOn)
    {
        if (isUpdatingToggles) return; // Skip if we're updating from code
        if (isOn) ToggleTreeMode();
    }

    private void OnMountainToggleChanged(bool isOn)
    {
        if (isUpdatingToggles) return; // Skip if we're updating from code
        if (isOn) ToggleMountainMode();
    }

    private void OnWallToggleChanged(bool isOn)
    {
        if (isUpdatingToggles) return; // Skip if we're updating from code
        if (isOn) ToggleWallMode();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
