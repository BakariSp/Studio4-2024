using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeController : MonoBehaviour
{
    [SerializeField] private List<UnityEngine.UI.Toggle> toggles = new List<UnityEngine.UI.Toggle>();

    void Start()
    {
        // Add listeners to all toggles
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener((isOn) => OnToggleValueChanged(toggle, isOn));
        }
    }

    private void OnToggleValueChanged(UnityEngine.UI.Toggle changedToggle, bool isOn)
    {
        if (isOn)
        {
            // Turn off all other toggles except the one that was just turned on
            foreach (var toggle in toggles)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
