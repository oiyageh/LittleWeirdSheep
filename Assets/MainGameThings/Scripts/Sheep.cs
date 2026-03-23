using UnityEngine;

[System.Serializable]
public class SheepData
{
    public string sheepName;
    public bool mustBeRed;
}

public class Sheep : MonoBehaviour
{
    public SheepData data;

    [HideInInspector]
    public int currentColorIndex = -1;

    void Start()
    {
        if (DecalStats.Instance != null)
            DecalStats.Instance.RegisterSheep(this);
    }

    public void SetColor(int colorIndex)
    {
        currentColorIndex = colorIndex;

        if (DecalStats.Instance != null)
        {
            DecalStats.Instance.UpdateSheepColor(this, colorIndex);
            Debug.Log($"Sheep {GetSheepName()} set to color {colorIndex}");
        }

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

        return gameObject.name;
    }

    public bool MustBeRed()
    {
        return data != null && data.mustBeRed;
    }
}