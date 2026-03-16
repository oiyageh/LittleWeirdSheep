using UnityEngine;
using System.Collections.Generic;

public class SheepManager : MonoBehaviour
{
    public static SheepManager Instance;

    public List<ISheepData> stampedEnemies = new List<ISheepData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);//keep it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddEnemy(ISheepData enemy)
    {
        stampedEnemies.Add(enemy);
    }
}
