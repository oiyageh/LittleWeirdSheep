using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIButtons : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "GameScene";
    public string mainMenuSceneName = "MainMenu";

    //  Replay
    public void ReplayGame()
    {
        if (DecalStats.Instance != null)
        {
            DecalStats.Instance.ResetStats();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    //  Main Menu
    public void GoToMainMenu()
    {
        if (DecalStats.Instance != null)
        {
            DecalStats.Instance.ResetStats();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}