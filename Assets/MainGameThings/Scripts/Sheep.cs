using UnityEngine;

[System.Serializable]
public class SheepData
{
    public string sheepName;    // Name shown in summary
    public bool mustBeRed;      // Required to be red
}

public class Sheep : MonoBehaviour
{
    [Header("Sheep Info")]
    public SheepData data;       // Set in Inspector

    [HideInInspector]
    public int currentColorIndex = -1; // -1 = no decal

    void OnEnable()
    {
        if (DecalStats.Instance != null)
            DecalStats.Instance.RegisterSheep(this);
    }

    void OnDisable()
    {
        if (DecalStats.Instance != null)
            DecalStats.Instance.UnregisterSheep(this);
    }

    public void SetColor(int colorIndex)
    {
        currentColorIndex = colorIndex;
    }

    public void RemoveColor()
    {
        currentColorIndex = -1;
    }

    public string GetColorName()
    {
        switch (currentColorIndex)
        {
            case 0: return "Red";
            case 1: return "Blue";
            case 2: return "Green";
            default: return "None";
        }
    }

    public string GetSheepName()
    {
        return data.sheepName;
    }

    public bool MustBeRed()
    {
        return data.mustBeRed;
    }
}