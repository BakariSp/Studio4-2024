using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenStateManager : MonoBehaviour
{
    [SerializeField]
    private Transform targetTransform; // The transform to sync with

    [SerializeField]
    private GameObject pencilDrawer;
    private GameObject pencilTip;

    [SerializeField]
    private LineDrawer lineDrawer; // Reference to the LineDrawer

    void Start()
    {
        if(pencilDrawer == null)
        {
            Debug.LogError("Pencil Drawer is not assigned");
        }

        // Find the child object named "PencilTip"
        pencilTip = pencilDrawer.transform.Find("PencilTip")?.gameObject;
        if(pencilTip == null)
        {
            Debug.LogError("Pencil Tip is not assigned");
        }

        pencilDrawer.SetActive(false);
        pencilTip.SetActive(false);
    }

    public void EnablePencilDrawer()
    {
        pencilDrawer.SetActive(true);
        pencilTip.SetActive(true);
        lineDrawer.StartNewLine(pencilTip); // Start a new line when the pencil tip is activated
    }

    public void DisablePencilDrawer()
    {
        pencilDrawer.SetActive(false);
        pencilTip.SetActive(false);
        // lineDrawer.RemoveLine(pencilTip); // Remove the current line when the pencil tip is deactivated
    }
    
    public void SwitchState()
    {
        bool isActive = !pencilDrawer.activeSelf;
        pencilDrawer.SetActive(isActive);
        pencilTip.SetActive(isActive);
        if (isActive)
        {
            lineDrawer.StartNewLine(pencilTip); // Start a new line when the pencil tip is activated
        }
        // else
        // {
        //     lineDrawer.RemoveLine(pencilTip); // Remove the current line when the pencil tip is deactivated
        // }
    }
    
}
