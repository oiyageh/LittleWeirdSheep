using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading/restarting scenes

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference to your Pause Menu UI Panel
    public static bool GameIsPaused = false; // A static boolean to check the pause state

    void Start()
    {
        // Ensure the pause menu is hidden when the game starts
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for an input key (e.g., Escape key) to toggle the pause state
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Public function to resume the game
    public void Resume()
    {
        Debug.Log("Resumed game");
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false); // Hide the pause menu UI
        }
        Time.timeScale = 1f; // Resume time
        // Resume all audio
        AudioListener.pause = false;
        GameIsPaused = false;
        //ensure the cursor is locked and hidden again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Public function to pause the game
    void Pause()
    {
        Debug.Log("PAUSED GAME!");
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true); // Show the pause menu UI
        }
        Time.timeScale = 0f; // Stop time

        //  Pause all audio
        AudioListener.pause = true;
        GameIsPaused = true;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Example of another menu function
    public void LoadMenu()
    {
        Time.timeScale = 1f; // Always resume time before loading a new scene
        AudioListener.pause = false; // safety reset
        SceneManager.LoadScene("MainMenu");
        Debug.Log("going to main menu");
    }

    // Example of a quit game function
    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit(); // Quits the application (works in builds, not in editor)
    }
}
