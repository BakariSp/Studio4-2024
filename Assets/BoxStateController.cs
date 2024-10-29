using UnityEngine;

public class BoxStateController : MonoBehaviour
{
    private LineDrawer lineDrawer;

    void Start()
    {
        lineDrawer = FindObjectOfType<LineDrawer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pen"))
        {
            if (lineDrawer != null)
            {
                lineDrawer.InsideBoxCollider = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
       if (other.gameObject.CompareTag("Pen"))
        {
            if (lineDrawer != null)
            {
                lineDrawer.InsideBoxCollider = false;
            }
        }
    }
}
