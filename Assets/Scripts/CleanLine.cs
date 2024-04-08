using UnityEngine;
using System.Collections;

public class CleanLine : MonoBehaviour {

    public LineRenderer lineRenderer;

    void OnTriggerEnter(Collider other) 
    {
        ClearLines();
    }
    public void ClearLines()
    {
        if (lineRenderer != null)
        {

            lineRenderer.positionCount = 0;
        }
        Debug.Log("clean line!");
    }

}

