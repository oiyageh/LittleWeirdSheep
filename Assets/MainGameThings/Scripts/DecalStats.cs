using UnityEngine;
using System.Collections.Generic;

public class DecalStats : MonoBehaviour
{
    public static DecalStats Instance;

    public int redCount;
    public int blueCount;
    public int greenCount;

    private List<Sheep> allSheep = new List<Sheep>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Color counting for UI
    public void AddColor(int index)
    {
        if (index == 0) redCount++;
        else if (index == 1) blueCount++;
        else if (index == 2) greenCount++;
    }

    public void RemoveColor(int index)
    {
        if (index == 0) redCount--;
        else if (index == 1) blueCount--;
        else if (index == 2) greenCount--;
    }

    // Sheep registration
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

    // Win check: all required sheep must be red
    public bool AllCorrectSheepAreRed()
    {
        foreach (Sheep sheep in allSheep)
        {
            if (sheep == null) continue;

            if (sheep.mustBeRed && !sheep.isCorrect) return false;
            if (!sheep.mustBeRed && sheep.isCorrect) return false;
        }

        return true;
    }

    // Get all sheep for UI
    public List<Sheep> GetAllSheep()
    {
        return allSheep;
    }
}
