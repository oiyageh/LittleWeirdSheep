using UnityEngine;

public class SheepParadeWalker : MonoBehaviour
{
    public float moveSpeed = 3f;
    private Vector3 targetPos;
    private bool hasTarget = false;

    public void Setup(Vector3 endPoint, SheepSaveData data)
    {
        targetPos = endPoint;
        hasTarget = true;

        // Apply visual stamp here if needed
        if (data.isBad)
        {
            // Logic to show mutant model or stamp
        }
    }

    void Update()
    {
        if (!hasTarget) return;

        // Move the sheep toward the end point
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Make the sheep face the end point
        Vector3 direction = (targetPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
        }

        // Destroy when reached
        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            Destroy(gameObject);
        }
    }
}