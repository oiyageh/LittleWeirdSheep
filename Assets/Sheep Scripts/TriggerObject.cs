using UnityEngine;

public class TriggerObject : MonoBehaviour
{
    [Header("Trigger Settings")]
    public float triggerRadius = 8f;
    public float lifeAfterPlaced = 5f;
    public LayerMask badSheepLayer;

    [Header("State")]
    public bool isHeld = false;
    public bool hasBeenPlaced = false;

    Rigidbody rb;
    Collider mainCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCollider = GetComponent<Collider>();
    }

    public void PickUp(Transform holdPoint)
    {
        if (hasBeenPlaced) return;

        isHeld = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Place()
    {
        if (hasBeenPlaced) return;

        isHeld = false;
        hasBeenPlaced = true;

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        TriggerNearbyBadSheep();

        Destroy(gameObject, lifeAfterPlaced);
    }

    void TriggerNearbyBadSheep()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius, badSheepLayer);

        foreach (Collider hit in hits)
        {
            BadSheepAI badSheep = hit.GetComponentInParent<BadSheepAI>();
            if (badSheep != null)
            {
                badSheep.TriggerBadSheep();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}