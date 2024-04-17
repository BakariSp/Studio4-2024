using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SetItem : MonoBehaviour
{
    public GameObject[] items;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVisible() 
    {
        foreach(GameObject child in items)
        {
            child.SetActive(true);
        }
    }

    public void SetInvisible() 
    {
        foreach(GameObject child in items)
        {
            child.SetActive(false);
        }
    }
}
