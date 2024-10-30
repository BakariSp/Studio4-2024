using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class DebugDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private int maxLines = 10;
    private Queue<string> debugLines = new Queue<string>();
    
    private static DebugDisplay instance;
    public static DebugDisplay Instance => instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDebugMessage(string message)
    {
        if (debugLines.Count >= maxLines)
        {
            debugLines.Dequeue();
        }
        
        debugLines.Enqueue($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
        UpdateDebugText();
    }

    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = string.Join("\n", debugLines);
        }
    }

    public void ClearDebug()
    {
        debugLines.Clear();
        UpdateDebugText();
    }
}
