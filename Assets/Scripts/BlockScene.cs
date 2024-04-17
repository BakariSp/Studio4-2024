using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockScene", menuName = "Block Scene", order = 51)]
public class BlockScene : ScriptableObject
{
    public string Name;
    public GameObject Scene;
    public Material Skybox;
    public AudioClip SoundClip;
}
