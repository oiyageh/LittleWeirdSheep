using UnityEngine;

[CreateAssetMenu(fileName = "Sheep", menuName = "Game Data/Sheep Data")]
public class ISheepData : ScriptableObject
{
    public string sheepName;
    public Color stampColor;
    public bool isWeird;
    // Add more fieldsif needed
}