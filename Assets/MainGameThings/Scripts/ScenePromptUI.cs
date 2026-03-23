using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePromptUI : MonoBehaviour
{
    public GameObject promptUI;
    public string sceneToLoad;

    public void OnYesPressed()
    {
        // 1. Resume time so the next scene isn't frozen
        Time.timeScale = 1f;

        // 2. NEW LINE: Tell GameManager to save all sheep names/data right now!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AssignNamesAndCollectSheep();
        }

        // 3. Now it is safe to change the scene
        SceneManager.LoadScene(sceneToLoad);

        AudioListener.pause = false; // Changed to false so you can hear the next scene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnNoPressed()
    {
        promptUI.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}