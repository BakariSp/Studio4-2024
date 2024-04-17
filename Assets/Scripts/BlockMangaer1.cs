using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class BlockManager1 : MonoBehaviour
{
    public Material[] materials;
    public BlockScene[] blockScenes;
    public AudioSource audioSource; // Assign in the editor
    public AudioClip clip;
    [SerializeField] private GameObject themeText;
    public int maxSelectLenght = 4;
    private bool isPatternProcessing = false;
    private GameObject currentActiveScene;
    

    // public Transform trigggerObject;
    public Transform blockContainer;
    private Dictionary<int, int> blockTriggerCounts = new Dictionary<int, int>(); // To track how many times each block is triggered

    private List<int> blockSequence = new List<int>(); // To store the sequence of blocks
    private float timeSinceLastInteraction; // Time since the last block interaction
    private const float sequenceRefreshTime = 2f; // Time to refresh the sequence
    public float triggerCooldown = 0.2f;
    private float lastTriggerTime = -1f; // Time of the last trigger
    private bool isCooldownActive = false;


    public static BlockManager1 Instance { get; private set; }

    private void Awake()
    {
        blockContainer = transform.Find("blockContainer");
        if (themeText != null)
        {
            themeText.GetComponent<TMP_Text>().text = $"Started";
            StartCoroutine(HideTextAfterDelay(10));
        }
        else
        {
            Debug.LogWarning("Theme TextMeshPro component not assigned.");
        }
      
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (blockSequence.Count > 0 && (Time.time - timeSinceLastInteraction) > sequenceRefreshTime)
        {
            if (!isPatternProcessing) // Check if not currently processing a pattern
            {
                blockSequence.Clear(); // Refresh the sequence after 2 seconds of no new interactions
            }
        }
    }

    public void PlayBlockSound(int blockIndex)
    {
        if (Time.time - lastTriggerTime >= triggerCooldown)
        {
            lastTriggerTime = Time.time;
            audioSource.pitch = Mathf.Clamp(1.0f - (blockIndex - 1) * 0.05f, 0.5f, 1.5f);
            audioSource.volume = 3.0f;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void RecordBlockSequence(int blockIndex)
    {
        if (isCooldownActive || isPatternProcessing) return;
        if (blockSequence.Count == 0 || blockSequence[blockSequence.Count - 1] != blockIndex)
        {
            blockSequence.Add(blockIndex);
            SetBlockVisibility(blockIndex.ToString(), true); // Convert index to string for the name
            UpdateBlockMaterial(blockIndex);

            timeSinceLastInteraction = Time.time;

            if (blockSequence.Count >= maxSelectLenght)
            {
                isCooldownActive = true;
                StartCoroutine(DelayedSequenceProcess());
            }
        }
    }

    private IEnumerator DelayedSequenceProcess()
    {
        isPatternProcessing = true; // Set flag to true to indicate pattern processing is starting

        yield return new WaitForSeconds(1.0f); // Wait for 2 seconds before processing the sequence
        
        PatternCompleted();
        ResetBlockVisibility(); // Now resetting visibility after the delay
        blockSequence.Clear();
        blockTriggerCounts.Clear(); // Reset trigger counts after completing the pattern
        isCooldownActive = false;

        isPatternProcessing = false; // Reset flag to false after processing is complete
    }

     private void UpdateBlockMaterial(int blockIndex)
    {
        // Increment the trigger count for this block
        if (!blockTriggerCounts.ContainsKey(blockIndex))
        {
            blockTriggerCounts[blockIndex] = 0;
        }
        blockTriggerCounts[blockIndex]++;

        // Find the block by name
        Transform blockTransform = blockContainer.Find(blockIndex.ToString());
        if (blockTransform != null)
        {
            MeshRenderer renderer = blockTransform.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Choose the material based on the trigger count, clamping to the bounds of the materials array
                int materialIndex = Mathf.Clamp(blockTriggerCounts[blockIndex] - 1, 0, materials.Length - 1);
                renderer.material = materials[materialIndex];
            }
        }
    }

    private void SetBlockVisibility(string blockName, bool isVisible)
    {
        Transform blockTransform = blockContainer.Find(blockName);
        if (blockTransform != null)
        {
            // Here, we directly access the MeshRenderer to change visibility
            MeshRenderer renderer = blockTransform.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = isVisible;
            }
        }
    }

    private void ResetBlockVisibility()
    {
        foreach (var blockIndex in blockSequence)
        {
            SetBlockVisibility(blockIndex.ToString(), false);
        }
    }

    private void PatternCompleted()
    {
        // Interpret the sequence to determine the concept it represents
        string theme = InterpretSequence();
        Debug.Log($"Pattern completed! Theme: {theme}");
        // You could instantiate a visual effect or unlock a new level based on the theme here
        // Update the TextMeshPro text to display the theme
        if (themeText != null)
        {
            themeText.SetActive(true);
            themeText.GetComponent<TMP_Text>().text = $"Theme: {theme}";
            StartCoroutine(HideTextAfterDelay(1));
        }
        else
        {
            Debug.LogWarning("Theme TextMeshPro component not assigned.");
        }

        // Handle the theme interaction
        SwitchThemeInteraction(theme);

    }

    // private void SwitchThemeInteraction(string theme)
    // {
    //     switch (theme)
    //     {
    //         case "Mountain":
    //             SwitchScene(0); // Call for both Trees and Mountain
    //             ChangeSkybox(normalSkybox);
    //             PlaySceneSound(stone);
    //             break;
    //         case "Water":
    //             // Call function for Water theme
    //             SwitchScene(1);
    //             ChangeSkybox(waterSkybox);
    //             PlaySceneSound(water);
    //             break;
    //         case "Trees":
    //             SwitchScene(1);
    //             ChangeSkybox(treeSkybox);
    //             PlaySceneSound(tree);
    //             break;
    //         case "Wind":
    //             // Call function for Wind theme
    //             SwitchScene(1);
    //             ChangeSkybox(waterSkybox);
    //             PlaySceneSound(wind);
    //             break;
    //         case "Telescope":
    //             // Call function for Telescope theme
    //             SwitchScene(2);
    //             ChangeSkybox(starSkybox);
    //             PlaySceneSound(star);
    //             break;
    //         case "Stars":
    //             // Call function for Stars theme
    //             SwitchScene(2);
    //             ChangeSkybox(starSkybox);
    //             PlaySceneSound(star);
    //             break;
    //         default:
    //             SwitchScene(3);
    //             break;
    //     }
    // }

    private void SwitchThemeInteraction(string theme)
    {
        foreach (var blockScene in blockScenes)
        {
            if (blockScene.Name == theme)
            {
                // Deactivate the current scene
                if (currentActiveScene != null)
                {
                    Destroy(currentActiveScene);
                }

                // Instantiate the new scene prefab and activate it
                currentActiveScene = Instantiate(blockScene.Scene);
                currentActiveScene.SetActive(true);

                ChangeSkybox(blockScene.Skybox);
                PlaySceneSound(blockScene.SoundClip);
                break;
            }
        }
    }

    // private void SwitchScene(BlockScene blockScene)
    // {
    //     // Deactivate all current scenes
    //     foreach (var scene in blockScenes)
    //     {
    //         if(scene.Instance != null) // Assuming there's a property Instance holding the instantiated GameObject
    //         {
    //             scene.Instance.SetActive(false);
    //         }
    //     }

    //     // If the scene we want to activate is not instantiated, instantiate it
    //     if(blockScene.Instance == null)
    //     {
    //         blockScene.Instance = Instantiate(blockScene.Prefab);
    //     }

    //     // Activate the selected scene
    //     blockScene.Instance.SetActive(true);
    // }

    private void PlaySceneSound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }
    

    // private void SwitchScene(int index)
    // {
    //     for (int i=0; i < Scene.Length; i++) 
    //     {
    //         if (i != index)
    //         {
    //             Scene[i].SetActive(false);
    //         }
            
    //     }

    //     if (Scene[index] != null && Scene[index].activeSelf == false) 
    //     {
    //         Scene[index].SetActive(true);
    //     }
    // }


    public void ChangeSkybox(Material newSkybox)
    {
        if (newSkybox != null)
        {
            RenderSettings.skybox = newSkybox;
            // Optional: If the skybox change does not immediately reflect in the scene,
            // force an environment update.
            DynamicGI.UpdateEnvironment();
        }
        else
        {
            Debug.LogError("New Skybox material has not been assigned.");
        }
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        themeText.SetActive(false); // Make the text or canvas invisible again
    }

    

    private string InterpretSequence()
    {
        // First, calculate metrics for the current sequence
        SequenceMetrics metrics = CalculateSequenceMetrics(blockSequence);
        
        // Next, use these metrics to determine the most fitting theme
        return DetermineThemeBasedOnMetrics(metrics);
    }

    private SequenceMetrics CalculateSequenceMetrics(List<int> sequence)
    {
        int totalValue = sequence.Sum();
        int range = sequence.Max() - sequence.Min();
        int directionChanges = CalculateDirectionChanges(sequence);
        int uniqueValues = sequence.Distinct().Count();

        return new SequenceMetrics
        {
            TotalValue = totalValue,
            Range = range,
            DirectionChanges = directionChanges,
            UniqueValues = uniqueValues
        };
    }

    private int CalculateDirectionChanges(List<int> sequence)
    {
        int changes = 0;
        bool increasing = sequence[1] > sequence[0]; // Initial direction

        for (int i = 1; i < sequence.Count - 1; i++)
        {
            if ((sequence[i + 1] > sequence[i]) != increasing)
            {
                changes++;
                increasing = !increasing; // Change direction
            }
        }
        return changes;
    }

    private string DetermineThemeBasedOnMetrics(SequenceMetrics metrics)
    {
        // Decision logic based on calculated metrics

        // Example of a simple decision logic based on the metrics
        if (metrics.Range > 10 && metrics.UniqueValues == blockSequence.Count) return "story1";
        if (metrics.DirectionChanges > 2) return "story1";
        if (metrics.TotalValue > 40) return "story2";
        if (blockSequence.SequenceEqual(blockSequence.OrderBy(x => x))) return "story2";
        if (blockSequence[0] == blockSequence[blockSequence.Count - 1]) return "story3";
        if ((blockSequence[0] + blockSequence[blockSequence.Count - 1]) / 2 == blockSequence[blockSequence.Count / 2]) return "story3";

        return "Unknown";
    }

    // Definition for SequenceMetrics might remain the same as previously defined
    class SequenceMetrics
    {
        public int TotalValue { get; set; }
        public int Range { get; set; }
        public int DirectionChanges { get; set; }
        public int UniqueValues { get; set; }
    }

}
