using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private Vector3 offset = new Vector3(0.5f, -0.5f, 0f);

    void Start()
    {
        // If camera reference is not set, get the main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Update position to stay in front of camera
        Vector3 targetPosition = mainCamera.transform.position + 
                               mainCamera.transform.forward * distanceFromCamera + 
                               offset;
        
        transform.position = targetPosition;
    }

    

}
