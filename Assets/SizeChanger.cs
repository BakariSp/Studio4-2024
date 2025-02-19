using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeChanger : MonoBehaviour
{
    public float sizeChangeSpeed = 0.1f; // Speed of size change
    public float moveSpeed = 1f; // Speed of movement
    public float minSize = 0.5f; // Minimum size
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Method to increase the size of the game object by 0.2
    public void IncreaseSize()
    {
        if (transform.localScale.x < 10f)
        {
            transform.localScale += new Vector3(sizeChangeSpeed, sizeChangeSpeed, sizeChangeSpeed);
        }else{
            transform.localScale += new Vector3(sizeChangeSpeed*10f, sizeChangeSpeed*10f, sizeChangeSpeed*10f);
        }
        
    }

    public void DecreaseSize()
    {
        if (transform.localScale.x > 10f)
        {
            transform.localScale -= new Vector3(sizeChangeSpeed*10f, sizeChangeSpeed*10f, sizeChangeSpeed*10f);
        }
        else if (transform.localScale.x > minSize)
        {
            transform.localScale -= new Vector3(sizeChangeSpeed, sizeChangeSpeed, sizeChangeSpeed);
        }
        
    }

    public void MoveUp()
    {
        transform.position += new Vector3(0, moveSpeed, 0);
    }

    
}
