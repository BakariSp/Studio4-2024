using UnityEngine;

public class RaycastController : MonoBehaviour
{
    [SerializeField] private Camera userCamera;
    [SerializeField] private GameObject fingerObject;
    [SerializeField] private Material raycastLineMaterial;
    
    private GameObject currentDebugLine;

    public struct RaycastResult
    {
        public bool success;
        public Vector3 hitPoint;
        public GameObject debugLine;
    }

    public RaycastResult CastRayFromCamera()
    {
        RaycastResult result = new RaycastResult();
        
        if (userCamera == null || fingerObject == null)
        {
            Debug.LogError("RaycastController: Missing camera or finger reference!");
            return result;
        }

        // Cast ray from camera through finger position
        Vector3 rayDirection = (fingerObject.transform.position - userCamera.transform.position).normalized;
        Ray ray = new Ray(userCamera.transform.position, rayDirection);
        RaycastHit hit;

        // Clean up previous debug line if it exists
        if (currentDebugLine != null)
        {
            Destroy(currentDebugLine);
        }
        
        // Create debug line
        currentDebugLine = new GameObject("DebugRaycast");
        LineRenderer lineRenderer = currentDebugLine.AddComponent<LineRenderer>();
        lineRenderer.material = raycastLineMaterial != null ? raycastLineMaterial : new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.yellow;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Visualize the raycast
            lineRenderer.positionCount = 3;
            lineRenderer.SetPosition(0, userCamera.transform.position);
            lineRenderer.SetPosition(1, fingerObject.transform.position);
            lineRenderer.SetPosition(2, hit.point);
            
            result.success = true;
            result.hitPoint = hit.point;
            result.debugLine = currentDebugLine;
        }
        else
        {
            Destroy(currentDebugLine);
            result.success = false;
        }
        
        return result;
    }

    public void ClearDebugLine()
    {
        if (currentDebugLine != null)
        {
            Destroy(currentDebugLine);
        }
    }
}