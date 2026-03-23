using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DecalStats : MonoBehaviour
{
    public static DecalStats Instance;

    private List<Sheep> allSheep = new List<Sheep>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void RegisterSheep(Sheep sheep)
    {
        if (!allSheep.Contains(sheep))
            allSheep.Add(sheep);
    }

    public void UnregisterSheep(Sheep sheep)
    {
        if (allSheep.Contains(sheep))
            allSheep.Remove(sheep);
    }

    public List<Sheep> GetAllSheep() => allSheep;

    public bool CheckWinCondition()
    {
        foreach (Sheep sheep in allSheep)
        {
            if (sheep == null) continue;

            // Required sheep must be red
            if (sheep.MustBeRed() && sheep.currentColorIndex != 0)
                return false;

            // Optional sheep must be green
            if (!sheep.MustBeRed() && sheep.currentColorIndex != 2)
                return false;
        }
        return true;
    }

    // Call to evaluate and go to the correct scene
    public void EvaluateWinLose(string winScene, string loseScene)
    {
        if (CheckWinCondition()) SceneManager.LoadScene(winScene);
        else SceneManager.LoadScene(loseScene);
    }
}