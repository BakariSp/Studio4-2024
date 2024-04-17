using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Material newMaterial; // Assign this in the editor
    public Material baseMaterial;
    // private Transform t =  gameObject.GetComponent<Transform>();

    // Call this function to change the cube's material
    public void OnSelect()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        
        if (renderer != null)
        {
            renderer.material = newMaterial;
            Vector3 scaleToSet = new Vector3(0.8f, 0.8f, 0.8f);
            transform.localScale = scaleToSet;
            transform.localPosition = new Vector3(0f,0f,0f);
        }
        else
        {
            Debug.LogWarning("MeshRenderer component not found on the object.");
        }
    }

    public void UnSelect()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = baseMaterial;
            Vector3 scaleToSet = new Vector3(1.0f, 1.0f, 1.0f);
            transform.localScale = scaleToSet;
            transform.localPosition = new Vector3(0f,0f,0f);
        }
        else
        {
            Debug.LogWarning("MeshRenderer component not found on the object.");
        }
    }
}
