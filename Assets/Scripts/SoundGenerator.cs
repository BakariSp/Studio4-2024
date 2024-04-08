using UnityEngine;
using System.Collections.Generic;

public class SoundGenerator : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip[] soundClips; // Assign this in the inspector with your sound clips

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Public method to trigger sound playback based on an externally calculated slope
    public void PlaySoundForSlope(float slope)
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            int clipIndex = DetermineClipIndex(slope);
            if (clipIndex >= 0 && clipIndex < soundClips.Length)
            {
                audioSource.clip = soundClips[clipIndex];
                audioSource.Play();
            }
        }
    }

    private int DetermineClipIndex(float slope)
    {
        // Dynamically calculate step size based on the number of audio clips
        float stepSize = 1f / soundClips.Length;
        int clipIndex = Mathf.FloorToInt(slope / stepSize);

        return clipIndex;
    }

    // Example method showing how you might trigger sounds based on a manually set slope
    // This could be called from anywhere in your code, such as in response to user input or other events
    public void TriggerSoundBasedOnSlope(float _slope)
    {
        PlaySoundForSlope(_slope);
    }
}
