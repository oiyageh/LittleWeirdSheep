using UnityEngine;
using System.Collections;

public class NightSheepSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform endPoint;
    public float spawnDelay = 2f;

    IEnumerator Start()
    {
        foreach (ISheepData enemy in SheepManager.Instance.stampedEnemies)
        {
            GameObject obj = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            obj.GetComponent<NightSheep>().Setup(enemy);
            obj.GetComponent<NightSheep>().endPoint = endPoint;
            yield return new WaitForSeconds(spawnDelay);
        }
    }
}