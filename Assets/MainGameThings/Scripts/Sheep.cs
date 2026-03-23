using UnityEngine;

public class Sheep : MonoBehaviour
{
    [Header("Sheep Info")]
    public string sheepName;        // Set the sheep’s name in Inspector

    [Header("Requirement")]
    public bool mustBeRed;          // Check this if this sheep must be red to “win”

    [HideInInspector]
    public bool isCorrect = false;  // Tracks if this sheep has correct color

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

        // Correct only if it must be red AND is painted red
        isCorrect = (mustBeRed && colorIndex == 0);
    }

    public void RemoveColor()
    {
        currentColorIndex = -1;
        isCorrect = false;
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
}