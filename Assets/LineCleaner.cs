using UnityEngine;
using System.Collections.Generic;

public class LineCleaner : MonoBehaviour
{
    public LineDrawer lineDrawer;

    public void CleanAllLines()
    {
        if (lineDrawer != null)
        {
            lineDrawer.CleanAllLines();
        }
        else
        {
            Debug.LogError("LineDrawer reference is not set in LineCleaner.");
        }
    }
}
