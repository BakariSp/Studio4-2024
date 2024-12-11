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
    [SerializeField] private GameObject myPositionObject;

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
        Color targetColor;
        // Case 1: Object and color specified
        if (info.Length > 1 && 
            Enum.TryParse(info[0], true, out Object parsedObject) &&
            ColorUtility.TryParseHtmlString(info[1], out targetColor))
        {
            var objectController = controllers.FirstOrDefault(c => c.Object == parsedObject);
            if (objectController != null)
            {
                objectController.SetColor(targetColor);
            }
        }
        // Case 2: Only color specified (try to use "it" or "this" as target)
        else if (info.Length > 0 && ColorUtility.TryParseHtmlString(info[0], out targetColor))
        {
            var defaultObject = controllers.FirstOrDefault(c => c.Object == Object.It || c.Object == Object.This);
            if (defaultObject != null)
            {
                defaultObject.SetColor(targetColor);
            }
            else
            {
                // Fallback: affect all objects if no "it" or "this" object is found
                foreach (var controller in controllers)
                {
                    controller.SetColor(targetColor);
                }
            }
        }
    }

    public void SetRotation(string[] info)
    {
        DisplayValues("SetRotation:", info);
        // Case 1: Object and descriptive rotation specified
        if (info.Length > 1 && Enum.TryParse(info[0], true, out Object targetObject))
        {
            string rotationDescription = string.Join(" ", info.Skip(1));
            float rotationAmount = ParseRotationDescription(rotationDescription);
            
            var objectController = controllers.FirstOrDefault(c => c.Object == targetObject);
            if (objectController != null)
            {
                objectController.RotateTo(rotationAmount);
            }
        }
        // Case 2: Only rotation specified (try to use "it" or "this" as target)
        else if (info.Length > 0)
        {
            float rotationAmount = ParseRotationDescription(info[0]);
            var defaultObject = controllers.FirstOrDefault(c => c.Object == Object.It || c.Object == Object.This);
            if (defaultObject != null)
            {
                defaultObject.RotateTo(rotationAmount);
            }
            else
            {
                // Fallback: affect all objects if no "it" or "this" object is found
                foreach (var controller in controllers)
                {
                    controller.RotateTo(rotationAmount);
                }
            }
        }
    }

    private float ParseRotationDescription(string description)
    {
        description = description.ToLower();
        
        // Default rotation amounts
        if (description.Contains("little") || description.Contains("bit"))
            return 15f;
        if (description.Contains("lot") || description.Contains("much"))
            return 90f;
        if (description.Contains("completely") || description.Contains("around"))
            return 180f;
        
        // If no matching description, try to parse as number
        if (float.TryParse(description, out float rotation))
            return rotation;
            
        // Default rotation if nothing matches
        return 45f;
    }

    public void MoveObject(string[] info)
    {
        DisplayValues("MoveObject:", info);
        Direction parsedDirection;
        // Case 1: Object and direction specified
        if (info.Length > 1 && 
            Enum.TryParse(info[0], true, out Object parsedObject) &&
            Enum.TryParse(info[1], true, out parsedDirection))
        {
            var objectController = controllers.FirstOrDefault(c => c.Object == parsedObject);
            if (objectController != null)
            {
                objectController.MoveDirection(parsedDirection, myPositionObject ? myPositionObject : null);
            }
        }
        // Case 2: Only direction specified (try to use "it" or "this" as target)
        else if (info.Length > 0 && Enum.TryParse(info[0], true, out parsedDirection))
        {
            var defaultObject = controllers.FirstOrDefault(c => c.Object == Object.It || c.Object == Object.This);
            if (defaultObject != null)
            {
                defaultObject.MoveDirection(parsedDirection, myPositionObject ? myPositionObject : null);
            }
            else
            {
                // Fallback: affect all objects if no "it" or "this" object is found
                foreach (var controller in controllers)
                {
                    controller.MoveDirection(parsedDirection, myPositionObject ? myPositionObject : null);
                }
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
    It,
    This,
    Rock,
}

public enum Direction
{
    Forward,
    Backward,
    Left,
    Right,
    Up,
    Down,
    Here,
    Closer,
    Further,
}

public enum Mode
{
    WallMode,
    TreeMode,
    MountainMode,
}


