using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveBlock : MonoBehaviour
{
    public Transform detached_object;
    private Transform t;
    // Start is called before the first frame update
    void Start()
    {
        t = transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (detached_object != null) 
        {
            // Position
            Vector3 newPosition = new Vector3(detached_object.transform.localPosition.x, t.localPosition.y, detached_object.transform.localPosition.z);
            t.localPosition = newPosition;

            // Rotation (Horizontal/Yaw)
            // Get the current rotation in Euler angles, modify the y-component, then set it back
            Vector3 newRotation = t.localEulerAngles;
            newRotation.y = detached_object.transform.localEulerAngles.y; // Copy the y-component (yaw) of the rotation
            t.localEulerAngles = newRotation;
        }
    }

}
