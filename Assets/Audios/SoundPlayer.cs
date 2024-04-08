using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public AudioClip clipToPlay; // The sound clip to play

    // Call this function to play the sound
    public void PlaySound()
    {
        if(audioSource != null && clipToPlay != null)
        {
            audioSource.clip = clipToPlay; // Assign the clip to play
            audioSource.Play(); // Play the clip
        }
        else
        {
            Debug.LogError("AudioSource or AudioClip is missing!");
        }
    }


}