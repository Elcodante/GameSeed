using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuManager : MonoBehaviour
{
    public void MulaiGame()
    {
        SceneManager.LoadScene("In_Game"); // Load the scene named "In_Game"
    }
}
