using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SheepRuntimeData
{
    public string sheepName;
    public bool mustBeRed;
    public int colorIndex;
}

public class DecalStats : MonoBehaviour
{
    public static DecalStats Instance;

    private List<SheepRuntimeData> allSheepData = new List<SheepRuntimeData>();

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
        foreach (var s in allSheepData)
        {
            if (s.sheepName == sheep.GetSheepName())
                return; // already registered
        }

        SheepRuntimeData data = new SheepRuntimeData
        {
            sheepName = sheep.GetSheepName(),
            mustBeRed = sheep.MustBeRed(),
            colorIndex = sheep.currentColorIndex
        };

        allSheepData.Add(data);
    }

    public void UpdateSheepColor(Sheep sheep, int colorIndex)
    {
        foreach (var s in allSheepData)
        {
            if (s.sheepName == sheep.GetSheepName())
            {
                s.colorIndex = colorIndex;
                return;
            }
        }
    }

    public List<SheepRuntimeData> GetAllSheep() => allSheepData;

    public bool CheckWinCondition()
    {
        foreach (var sheep in allSheepData)
        {
            if (sheep.mustBeRed && sheep.colorIndex != 0)
                return false;

            if (!sheep.mustBeRed && sheep.colorIndex != 2)
                return false;
        }
        return true;
    }

    public void EvaluateWinLose(string winScene, string loseScene)
    {
        if (CheckWinCondition()) SceneManager.LoadScene(winScene);
        else SceneManager.LoadScene(loseScene);
    }

    public void ResetStats()
    {
        allSheepData.Clear();
    }


}