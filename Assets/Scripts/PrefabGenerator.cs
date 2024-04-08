using UnityEngine;

public class PrefabGenerator : MonoBehaviour
{
    public GameObject[] prefabs; // Assign this in the inspector with your prefabs
    public Transform drawingObject; // The drawing object's transform
    public float scaleFactor = 0.3f;

    // This could be called whenever you want to generate a prefab based on the slope
    public void GeneratePrefabForSlope(float slope)
    {
        int prefabIndex = DeterminePrefabIndex(slope);
        if (prefabIndex >= 0 && prefabIndex < prefabs.Length)
        {
            InstantiatePrefab(prefabs[prefabIndex]);
        }
    }

    private int DeterminePrefabIndex(float slope)
    {
        // Dynamically calculate index based on the slope. Adjust logic as needed.
        float stepSize = 1f / prefabs.Length;
        int index = Mathf.FloorToInt(slope / stepSize);
        return Mathf.Clamp(index, 0, prefabs.Length - 1);
    }

    private void InstantiatePrefab(GameObject prefab)
    {
        if (prefab == null) 
        {
            return;
        }
        else 
        {
            Vector3 position = drawingObject.position;
            Quaternion rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)); // Random rotation around the Z axis
            Vector3 scale = Vector3.one * Random.Range(0.1f, 0.2f) * scaleFactor; // Random scale between 0.5 and 2

            GameObject instance = Instantiate(prefab, position, rotation);
            instance.transform.localScale = scale;
        }
        
    }
}
