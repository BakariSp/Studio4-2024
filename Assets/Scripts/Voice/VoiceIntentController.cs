using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using Oculus.Voice;
using System.Linq;
using System;

public class VoiceIntentController : MonoBehaviour
{
    [Header("Voice")]
    [SerializeField]
    private AppVoiceExperience appVoiceExperience;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI fullTranscriptText;
    [SerializeField]
    private TextMeshProUGUI partialTranscriptText;

    private ShapeController[] controllers;
    private bool appVoiceActive;

    [SerializeField] private BuildModeManager buildModeManager;

    private void Awake()
    {
        controllers = FindObjectsOfType<ShapeController>();
        fullTranscriptText.text = partialTranscriptText.text = string.Empty;
        
        // bind transcriptions and activate state
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener((transcription) =>
        {
            fullTranscriptText.text = transcription;
        });

        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener((transcription) =>
        {
            partialTranscriptText.text = transcription;
        });

        appVoiceExperience.VoiceEvents.OnRequestCreated.AddListener((req) =>
        {
            appVoiceActive = true;
            Debug.Log("Request created");
        });

        appVoiceExperience.VoiceEvents.OnComplete.AddListener((request) =>
        {
            appVoiceActive = false;
            Debug.Log("Request completed");
        });
    }


    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame && !appVoiceActive)
        {
            // activate voice experience
            appVoiceExperience.Activate();
        }
    }

    public void SetColor(string[] info)
    {
        DisplayValues("SetColor:", info);
        // set color info based on intent response
        if(info.Length > 0 && ColorUtility.TryParseHtmlString(info[0], out Color color))
        {
            foreach(var controller in controllers)
            {
                controller.SetColor(color);
            }
        }
    }

    public void SetRotation(string[] info)
    {
        DisplayValues("SetRotation:", info);
        // set rotation info based on intent response
        if(info.Length > 0 && float.TryParse(info[0], out float targetRotation))
        {
            foreach(var controller in controllers)
            {
                controller.RotateTo(targetRotation);
            }
        }
    }

    public void MoveObject(string[] info)
    {
        DisplayValues("MoveObject:", info);
        // move object info based on intent response
        if(info.Length > 1 && 
           Enum.TryParse(info[0], true, out Object targetObject) &&
           Enum.TryParse(info[1], true, out Direction direction))
        {
            var objectController = controllers.FirstOrDefault(c => c.Object == targetObject);
            if(objectController != null)
            {
                objectController.MoveDirection(direction);
            }
        }
    }

    public void SetMode(string[] info)
    {
        DisplayValues("SetMode:", info);
        if(info.Length > 0)
        {
            // Combine all words and remove spaces
            string modeString = string.Join("", info).Replace(" ", "");
            if(Enum.TryParse(modeString, true, out Mode mode))
            {
                switch (mode)
                {
                    case Mode.TreeMode:
                        buildModeManager.ToggleTreeMode();
                        break;
                    case Mode.WallMode:
                        buildModeManager.ToggleWallMode();
                        break;
                    case Mode.MountainMode:
                        buildModeManager.ToggleMountainMode();
                        break;
                    default:
                        Debug.LogWarning($"Unknown mode: {mode}");
                        break;
                }
            }
        }
    }

    private void DisplayValues(string title, string[] values)
    {
        string debug = title;
        foreach(var value in values)
        {
            debug += $" {value}";
        }
        Debug.Log(debug);
    }
}

public enum Object
{
    Cube,
    Tree,
    Wall,
}

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down,
}

public enum Mode
{
    WallMode,
    TreeMode,
    MountainMode,
}


