using UnityEngine;
using Oculus.Voice;
using Meta.WitAi;
using Meta.WitAi.Requests;
using TMPro;
using UnityEngine.UI;

public class VoiceController : MonoBehaviour
{
    [SerializeField] private VoiceService _voiceService;
    [SerializeField] private float volumeThreshold = 0.01f;
    [SerializeField] private int sampleDataLength = 1024;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private bool showVolumeDebug = true;
    private float maxVolumeObserved = 0f;
    private bool isListening = false;
    private string microphoneDevice;
    [SerializeField] private Button _activateButton;

    void Start()
    {
        statusText.text = "Waiting for voice...";
        // Get the default microphone device
        if (Microphone.devices.Length > 0)
        {
            microphoneDevice = Microphone.devices[0];
            // Set up audio source and start recording
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.clip = Microphone.Start(microphoneDevice, true, 1, AudioSettings.outputSampleRate);
            audioSource.loop = true;
            audioSource.Play(); // Play the audio source to monitor input
        }
        
        if (_voiceService == null)
        {
            statusText.text = "finding voice service...";
            _voiceService = FindObjectOfType<VoiceService>();
            statusText.text = "voice service found";
        }

        _activateButton = GetComponent<Button>();
    }

    void Update()
    {
        if (!isListening && microphoneDevice != null)
        {
            float volume = GetAverageVolume();
            
            // Track max volume
            if (volume > maxVolumeObserved)
            {
                maxVolumeObserved = volume;
            }

            // Update status text with volume information if debug is enabled
            if (showVolumeDebug)
            {
                statusText.text = $"Current: {volume:F4}\nMax: {maxVolumeObserved:F4}\nThreshold: {volumeThreshold}";
            }

            // If volume exceeds threshold, activate voice service
            if (volume > volumeThreshold)
            {
                statusText.text = "activating voice service...";
                _activateButton.onClick.Invoke();
            }
        }
    }

    private float GetAverageVolume()
    {
        float[] sampleData = new float[sampleDataLength];
        int micPosition = Microphone.GetPosition(microphoneDevice);
        AudioSource audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null && audioSource.clip != null && micPosition > 0)
        {
            // Make sure we don't read outside the buffer
            int startPosition = (micPosition - sampleDataLength) % audioSource.clip.samples;
            if (startPosition < 0) startPosition = 0;
            
            audioSource.clip.GetData(sampleData, startPosition);

            float sum = 0;
            for (int i = 0; i < sampleDataLength; i++)
            {
                sum += Mathf.Abs(sampleData[i]);
            }
            return sum / sampleDataLength;
        }
        
        return 0;
    }
}
