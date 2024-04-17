using UnityEngine;

public class BlockController1 : MonoBehaviour
{
    // Assuming this GameObject has a Renderer you want to control.
    private Renderer myRenderer;
    public float triggerCooldown = 0.4f; // 0.2 seconds cooldown
    private float lastTriggerTime = -1f; // Time of the last trigger

    // Start is called before the first frame update
    void Start()
    {
        // Get the Renderer component attached to this GameObject
        myRenderer = GetComponent<Renderer>();

        // Optionally, make the GameObject invisible at the start
        myRenderer.enabled = false; // Make invisible at the start
    }

    // OnTriggerEnter is called when the Collider other enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check for a specific tag if needed, for example:
        // if (other.CompareTag("Player"))
        // {
            // Make the GameObject visible when the trigger is activated
        // renderer.enabled = true;
        if (Time.time - lastTriggerTime >= triggerCooldown) {
            lastTriggerTime = Time.time;
            PlaySound();
        }
        
        // }
    }

    // private void OnCollisionEnter(Collision other) {
    //     if (Time.time - lastTriggerTime >= triggerCooldown) {
    //         lastTriggerTime = Time.time;
    //         PlaySound();
    //     }
    // }

    // Optional: Implement OnTriggerExit to hide the object again when an object exits the trigger
    // private void OnTriggerExit(Collider other)
    // {
    //     // Make the GameObject invisible again when the object exits the trigger
    //     renderer.enabled = false;
    // }


    private void PlaySound() {
        int blockIndex;
        if (int.TryParse(gameObject.name, out blockIndex))
        {
            // Call the BlockManager to play the sound for this block
            BlockManager1.Instance.PlayBlockSound(blockIndex);
            BlockManager1.Instance.RecordBlockSequence(blockIndex);
        }
    }
    
}
