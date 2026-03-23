using UnityEngine;
using System.Collections;

public class TriggerSpawner : MonoBehaviour
{
    [Header("Spawning Configuration")]
    public GameObject triggerPrefab;
    public float spawnInterval = 15f;
    public float checkRadius = 2f;
    public LayerMask triggerLayer; 

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (!IsSpotOccupied())
            {
                Instantiate(triggerPrefab, transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    bool IsSpotOccupied()
    {
        // Looks for any TriggerObject within the radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, triggerLayer);
        
        foreach (var col in colliders)
        {
            TriggerObject obj = col.GetComponent<TriggerObject>();
            // If there is an object there and it hasn't been picked up/placed yet, don't spawn
            if (obj != null && !obj.isHeld && !obj.hasBeenPlaced)
            {
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}