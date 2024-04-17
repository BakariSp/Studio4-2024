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

    public void QuitGame()
    {
        // Log a message to the console (useful for debugging)
        Debug.Log("Quit game request");

        // Check if we are running in the Unity editor
        #if UNITY_EDITOR
            // If running in the editor, stop playing the scene
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // If running in a build, quit the application
            Application.Quit();
        #endif
    }

}
