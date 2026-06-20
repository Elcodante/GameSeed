using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public static bool GameIsPaused = false; // Indicates whether the game is currently paused

    public GameObject pauseMenuUI; // Reference to the pause menu UI GameObject

    void Start()
    {
        Time.timeScale = 1f; // Ensure the game time is running at normal speed when the game starts
        pauseMenuUI.SetActive(false); // Hide the pause menu UI at the start of the game
    }

    void Update()
    {
        if(Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) // Check if the Escape key is pressed
        {
            if(GameIsPaused) // If the game is already paused
            {
                Resume(); // Resume the game
            }
            else // If the game is not paused
            {
                Pause(); // Pause the game
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu UI
        Time.timeScale = 1f; // Resume the game time
        GameIsPaused = false; // Update the paused state
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu UI
        Time.timeScale = 0f; // Pause the game time
        GameIsPaused = true; // Update the paused state
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // Resume the game time before loading the main menu
        SceneManager.LoadScene("Main_Menu"); // Load the main menu scene
    }

    public void RestardGame()
    {
        Time.timeScale = 1f; // Resume the game time before restarting the game
        GameIsPaused = false; // Update the paused state
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene to restart the game
    }
}
