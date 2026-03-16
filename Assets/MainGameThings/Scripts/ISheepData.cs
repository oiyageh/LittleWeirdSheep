using UnityEngine;

[CreateAssetMenu(fileName = "Sheep", menuName = "Game Data/Sheep Data")]
public class ISheepData : ScriptableObject
{
    public string SheepName;
    public StampType stampType;

    public Color GetStampColor()
    {
        switch (stampType)
        {
            case StampType.Hostile:
                return Color.red;

            case StampType.Innocent:
                return Color.green;

            case StampType.Unknown:
                return Color.blue;
        }

        return Color.white;
    }
}