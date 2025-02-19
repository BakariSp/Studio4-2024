using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PencilController : MonoBehaviour
{
    [SerializeField]
    private Transform targetTransform;

    [SerializeField]
    private Vector3 positionOffset; // Position offset for the pen

    [SerializeField]
    private Vector3 rotationOffset; // Rotation offset for the pen

    [SerializeField]
    private Transform PlayerTransform;


    // Update is called once per frame
    void Update()
    {
        if (targetTransform != null)
        {
            transform.position = targetTransform.TransformPoint(positionOffset);
            transform.rotation = targetTransform.rotation * Quaternion.Euler(rotationOffset);
        }
        if (PlayerTransform != null)
        {
            transform.localScale = PlayerTransform.localScale;
        }
    }


}
