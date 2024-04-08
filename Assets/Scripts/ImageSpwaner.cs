using UnityEngine;

public class ImageSpawner : MonoBehaviour
{
    public Transform spawnPos;

    public void SpawnImage(GameObject imagePrefab)
    {
        // Determine the interaction point. For OnTriggerEnter, it could be the collision point.
        // Here, we simply use the position of the object that triggered the event.
        Vector3 interactionPoint = spawnPos.position;

        // Calculate a random position around the interaction point
        Vector3 spawnPosition = interactionPoint + Random.insideUnitSphere * 5; // Adjust the multiplier as needed
        spawnPosition.y = 0; // Assuming you want to spawn on the ground in a 3D space

        // Instantiate the image prefab at the calculated position
        GameObject newImage = Instantiate(imagePrefab, spawnPosition, Quaternion.identity);

        // Set a random scale between 0.2 to 0.4
        float randomScale = Random.Range(0.2f, 0.4f);
        newImage.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
        Debug.Log("created image");

        // Destroy the new image after 3 seconds
        Destroy(newImage, 3f);
    }
}
