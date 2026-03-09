using UnityEngine;

public class PlayerTriggerCarrier : MonoBehaviour
{
    [Header("Pickup")]
    public float pickupRadius = 2.5f;
    public LayerMask triggerObjectLayer;
    public Transform holdPoint;
    public KeyCode interactKey = KeyCode.E;

    [Header("Debug")]
    public TriggerObject heldObject;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                PlaceHeldObject();
            }
        }
    }

    void TryPickUp()
    {
        Vector3 checkCenter = transform.position + Vector3.up * 0.8f;

        Collider[] hits = Physics.OverlapSphere(checkCenter, pickupRadius, triggerObjectLayer);

        if (hits.Length == 0)
        {
            Debug.Log("No trigger object found in pickup range.");
            return;
        }

        float closestDistance = Mathf.Infinity;
        TriggerObject closestObject = null;

        foreach (Collider hit in hits)
        {
            TriggerObject triggerObj = hit.GetComponentInParent<TriggerObject>();

            if (triggerObj != null && !triggerObj.hasBeenPlaced)
            {
                float dist = Vector3.Distance(transform.position, triggerObj.transform.position);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestObject = triggerObj;
                }
            }
        }

        if (closestObject != null)
        {
            heldObject = closestObject;
            heldObject.PickUp(holdPoint);
            Debug.Log("Picked up trigger object: " + closestObject.name);
        }
        else
        {
            Debug.Log("Found colliders, but no valid TriggerObject script.");
        }
    }

    void PlaceHeldObject()
    {
        if (heldObject == null) return;

        Vector3 placePos = transform.position + transform.forward * 1.5f;

        if (Physics.Raycast(placePos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
        {
            placePos = hit.point;
        }

        heldObject.transform.position = placePos;
        heldObject.transform.rotation = Quaternion.identity;
        heldObject.Place();
        Debug.Log("Placed trigger object: " + heldObject.name);
        heldObject = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 checkCenter = transform.position + Vector3.up * 0.8f;
        Gizmos.DrawWireSphere(checkCenter, pickupRadius);
    }
}