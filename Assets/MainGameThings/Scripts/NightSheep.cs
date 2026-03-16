using UnityEngine;

public class NightSheep : MonoBehaviour
{
    public Renderer stampRenderer;
    public Transform endPoint;
    public float walkSpeed = 2f;

    public void Setup(ISheepData data)
    {
        stampRenderer.material.color = data.GetStampColor();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            endPoint.position,
            walkSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, endPoint.position) < 0.1f)
            Destroy(gameObject);
    }
}
