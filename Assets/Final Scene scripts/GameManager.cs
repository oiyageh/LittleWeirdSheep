using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Sheep Settings")]
    public List<string> randomNames;

    [HideInInspector]
    public List<SheepSaveData> allSheepData = new List<SheepSaveData>();

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

    // This is the function the other scripts were failing to find
    public void AssignNamesAndCollectSheep()
    {
        allSheepData.Clear();

        // Using "Paintable" tag as requested
        GameObject[] sheepObjects = GameObject.FindGameObjectsWithTag("Paintable");
        List<string> namePool = new List<string>(randomNames);

        foreach (GameObject sheep in sheepObjects)
        {
            string chosenName;
            if (namePool.Count > 0)
            {
                int index = Random.Range(0, namePool.Count);
                chosenName = namePool[index];
                namePool.RemoveAt(index);
            }
            else
            {
                chosenName = "Sheep_" + Random.Range(1000, 9999);
            }

            // Detect if sheep is bad
            bool isBad = sheep.GetComponent<BadSheepAI>() != null;

            // Placeholder for stamp
            string stamp = "None";

            allSheepData.Add(new SheepSaveData(chosenName, isBad, stamp));
        }
        Debug.Log($"Successfully saved {allSheepData.Count} sheep data.");
    }
}