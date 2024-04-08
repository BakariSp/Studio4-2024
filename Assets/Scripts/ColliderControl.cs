using UnityEngine;
using System.Collections;

public class ColliderControl : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject imagePrefab;
    public AudioClip clipToPlay; // The sound clip to play
    public float audioSourceVolume = 1.0f;
    private ImageSpawner imageSpawner;


    void Start() {
        ImageSpawner imageSpawner = GameObject.Find("Spawner").GetComponent<ImageSpawner>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("clicked!");
        if (imageSpawner != null) {
            imageSpawner.SpawnImage(imagePrefab);
        }
        

        if(audioSource != null && clipToPlay != null)
        { 
            audioSource.clip = clipToPlay; // Assign the clip to play
            audioSource.Play(); // Play the clip
            
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("clicked!");
        if (imageSpawner != null) {
            imageSpawner.SpawnImage(imagePrefab);
        }
        

        if(audioSource != null && clipToPlay != null)
        { 
            audioSource.clip = clipToPlay; // Assign the clip to play
            audioSource.Play(); // Play the clip
            
        }
        
    }
}