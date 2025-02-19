using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxManager : MonoBehaviour
{
    public Material[] skyboxes; // Public array to hold skybox materials
    private int currentSkyboxIndex = 0; // Index to track the current skybox

    // Start is called before the first frame update
    void Start()
    {
        if (skyboxes.Length > 0)
        {
            RenderSettings.skybox = skyboxes[currentSkyboxIndex];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextSky()
    {
        if (skyboxes.Length == 0) return;

        currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxes.Length;
        RenderSettings.skybox = skyboxes[currentSkyboxIndex];
    }
}
