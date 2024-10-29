using UnityEngine;

public class DoorSurfaceGenerator : MonoBehaviour
{
    public LayerMask groundLayer; // Layer mask for the ground/floor mesh
    public float maxRayDistance = 100f; // Maximum distance for raycasting
    public float doorExtensionDistance = 10f; // How far to extend the door surface
    public GameObject doorPrefab;
    public float doorThickness = 0.2f;

    public GameObject GenerateDoorSurface(Vector3[] rectangleCorners, Camera userCamera)
    {
        // Ensure we have enough corners
        if (rectangleCorners.Length < 4) return null;

        // Sort corners to find bottom ones
        Vector3[] sortedCorners = SortCornersByHeight(rectangleCorners);
        Vector3 bottomLeft = sortedCorners[0];
        Vector3 bottomRight = sortedCorners[1];
        Vector3 topLeft = sortedCorners[2];
        Vector3 topRight = sortedCorners[3];

        // Get camera position
        Vector3 cameraPos = userCamera.transform.position;

        // Get ground intersection points for bottom corners
        Vector3 groundLeft = GetGroundIntersectionPoint(cameraPos, bottomLeft);
        Vector3 groundRight = GetGroundIntersectionPoint(cameraPos, bottomRight);

        if (groundLeft == Vector3.zero || groundRight == Vector3.zero)
        {
            Debug.LogWarning("Could not find ground intersection points");
            return null;
        }

        // Calculate the vertical surface normal
        Vector3 surfaceNormal = Vector3.Cross(Vector3.up, (groundRight - groundLeft).normalized);

        // Get the height using the middle point of TOP edge only
        Vector3 topMiddlePoint = (topLeft + topRight) * 0.5f;  // Changed to use only top edge points
        Vector3 doorTopMiddle = GetDoorTopPoint(cameraPos, topMiddlePoint, groundLeft, surfaceNormal);
        float doorHeight = doorTopMiddle.magnitude;

        // Create perfectly vertical top corners
        Vector3 doorTopLeft = groundLeft + Vector3.up * doorHeight;
        Vector3 doorTopRight = groundRight + Vector3.up * doorHeight;

        // Create door mesh
        GameObject doorObject = CreateDoorFromPrefab(groundLeft, groundRight, doorTopLeft, doorTopRight);

        return doorObject;
    }

    private Vector3[] SortCornersByHeight(Vector3[] corners)
    {
        // Create a copy of corners array to sort
        Vector3[] sortedCorners = new Vector3[corners.Length];
        corners.CopyTo(sortedCorners, 0);

        // Sort primarily by Y coordinate
        System.Array.Sort(sortedCorners, (a, b) => a.y.CompareTo(b.y));

        // The first two points (lowest Y) are our bottom points
        // Sort these two points by X coordinate
        if (sortedCorners[0].x > sortedCorners[1].x)
        {
            Vector3 temp = sortedCorners[0];
            sortedCorners[0] = sortedCorners[1];
            sortedCorners[1] = temp;
        }

        // Sort the top two points by X coordinate as well
        if (sortedCorners[2].x > sortedCorners[3].x)
        {
            Vector3 temp = sortedCorners[2];
            sortedCorners[2] = sortedCorners[3];
            sortedCorners[3] = temp;
        }

        return sortedCorners;
    }

    private Vector3 GetGroundIntersectionPoint(Vector3 cameraPos, Vector3 targetPoint)
    {
        Ray ray = new Ray(cameraPos, targetPoint - cameraPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private Vector3 GetDoorTopPoint(Vector3 cameraPos, Vector3 targetPoint, Vector3 groundPoint, Vector3 surfaceNormal)
    {
        Ray ray = new Ray(cameraPos, targetPoint - cameraPos);
        Plane verticalPlane = new Plane(surfaceNormal, groundPoint);
        float enter;

        if (verticalPlane.Raycast(ray, out enter))
        {
            Vector3 intersectionPoint = ray.GetPoint(enter);
            // Ensure we have a minimum height
            float heightFromGround = Vector3.Distance(intersectionPoint, groundPoint);
            if (heightFromGround < 1f) // Minimum 1 meter height
            {
                return groundPoint + Vector3.up * 1f;
            }
            return intersectionPoint;
        }

        // Fallback to a default height if raycast fails
        return groundPoint + Vector3.up * 2f; // Default 2 meter height
    }

    private GameObject CreateDoorFromPrefab(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topLeft, Vector3 topRight)
    {
        // Calculate door dimensions and position
        Vector3 center = (bottomLeft + bottomRight + topLeft + topRight) * 0.25f;
        float width = Vector3.Distance(bottomLeft, bottomRight);
        float height = Vector3.Distance(bottomLeft, topLeft);
        
        // Add validation for minimum dimensions
        width = Mathf.Max(width, 0.5f);  // Minimum 0.5 meter width
        height = Mathf.Max(height, 1f);   // Minimum 1 meter height
        
        // Calculate rotation
        Vector3 forward = Vector3.Cross(Vector3.up, (bottomRight - bottomLeft).normalized);
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);

        // Instantiate and configure door
        GameObject doorObject = Instantiate(doorPrefab, center, rotation);
        doorObject.name = "Door";
        
        // Set door scale to match dimensions
        doorObject.transform.localScale = new Vector3(width, height, doorThickness);

        return doorObject;
    }
}
