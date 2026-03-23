using UnityEngine;

[System.Serializable]
public class SheepData
{
    public string sheepName;   // <-- THIS must be filled in Inspector
    public bool mustBeRed;     // Required red sheep
}

public class Sheep : MonoBehaviour
{
    public SheepData data;         // Assign in Inspector

    [HideInInspector]
    public int currentColorIndex = -1; // -1 = not painted

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

    public string GetColorName()
    {
        return currentColorIndex switch
        {
            0 => "Red",
            1 => "Blue",
            2 => "Green",
            _ => "None"
        };
    }

    public string GetSheepName()
    {
        if (data != null && !string.IsNullOrEmpty(data.sheepName))
            return data.sheepName;
        else
            return gameObject.name;  // fallback if data not set
    }

    public bool MustBeRed()
    {
        return data != null && data.mustBeRed;
    }
}