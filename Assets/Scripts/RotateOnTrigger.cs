using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnTrigger : MonoBehaviour
{
    public GameObject RotateObject;
    // Start is called before the first frame update
    void OnTriggerEnter(Collider other) {
        Transform newTransform = RotateObject.GetComponent<Transform>();
        if (transform != null) {
            // Rotate the GameObject by 90 degrees around the Z-axis
            Quaternion rotationToAdd = Quaternion.Euler(0, 90, 0);
            newTransform.rotation = newTransform.rotation * rotationToAdd;
        }

    }
}
