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
        SetTransparency(0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (objectRenderer != null)
        {
            isEnabled = true;
            SetTransparency(0.8f);
            canvas.SetActive(isEnabled);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objectRenderer != null)
        {
            isEnabled = false;
            SetTransparency(0.1f);
            canvas.SetActive(isEnabled);
        }
    }

    private void SetTransparency(float alpha)
    {
        Color color = objectRenderer.material.color;
        color.a = alpha;
        objectRenderer.material.color = color;
    }
}
