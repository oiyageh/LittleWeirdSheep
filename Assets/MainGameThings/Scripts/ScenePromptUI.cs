using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePromptUI : MonoBehaviour
{
    public GameObject promptUI;
    public string sceneToLoad;

    public void OnYesPressed()
    {
        Time.timeScale = 1f; // Resume before loading
        SceneManager.LoadScene(sceneToLoad);
        AudioListener.pause = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnNoPressed()
    {
        promptUI.SetActive(false);
        Time.timeScale = 1f; // Resume game
        AudioListener.pause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}