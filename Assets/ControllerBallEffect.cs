using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerBallEffect : MonoBehaviour
{
    private Renderer objectRenderer;
    [SerializeField] private GameObject canvas;
    public bool isEnabled = false;

    private void Start()
    {
        if (objectRenderer == null) 
        {
            objectRenderer = GetComponent<Renderer>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (objectRenderer != null)
        {
            isEnabled = true;
            objectRenderer.enabled = isEnabled;
            canvas.SetActive(isEnabled);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectRenderer != null)
        {
            isEnabled = false;
            objectRenderer.enabled = isEnabled;
            canvas.SetActive(isEnabled);
        }
    }
}
