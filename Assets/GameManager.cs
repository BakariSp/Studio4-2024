using UnityEngine;
using UnityEngine.SceneManagement; // Import the SceneManagement namespace


public class GameManager : MonoBehaviour
{

    public void ResetScene()
    {
        // Gets the currently active scene and reloads it
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Game reseted");
    }

}
