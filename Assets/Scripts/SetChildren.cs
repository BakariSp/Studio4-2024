using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetChildren : MonoBehaviour
{
    // Function to set all children to visible
    public void SetVisible()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    // Function to set all children to invisible
    public void SetInvisible()
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
