using UnityEngine;
[System.Serializable]
public class SheepSaveData
{
    public string name;
    public bool isBad;
    public string stampType;
    public SheepSaveData(string name, bool isBad, string stamp)
    {
        this.name = name;
        this.isBad = isBad;
        this.stampType = stamp;
    }
}