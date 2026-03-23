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

    void Start()
    {
        // Ensure the object falls to the ground when first spawned
        if (rb != null && !isHeld)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
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

        // We set to kinematic here so it stays exactly where you put it
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
            // Assuming BadSheepAI script exists on the targets
            var badSheep = hit.GetComponentInParent<MonoBehaviour>();
            if (badSheep != null)
            {
                badSheep.Invoke("TriggerBadSheep", 0);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}