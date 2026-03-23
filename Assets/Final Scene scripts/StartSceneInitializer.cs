using UnityEngine;

public class StartSceneInitializer : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AssignNamesAndCollectSheep();
        }
        else
        {
            Debug.LogError("No GameManager found in the scene!");
        }
    }
}