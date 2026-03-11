using UnityEngine;
using UnityEngine.AI;

public class SheepAnimationController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public NavMeshAgent agent;

    [Header("Settings")]
    public bool useNavMeshAgent = true;
    public float walkThreshold = 0.03f;
    public bool usePositionCheckBackup = true;

    private Vector3 lastPosition;
    private float worldMoveSpeed;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (useNavMeshAgent && agent == null)
            agent = GetComponent<NavMeshAgent>();

        lastPosition = transform.position;
    }

    void Update()
    {
        float movementValue = 0f;

        // Backup: check real world movement
        worldMoveSpeed = (transform.position - lastPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = transform.position;

        if (useNavMeshAgent && agent != null)
        {
            // desiredVelocity is often better for AI animation than velocity
            movementValue = agent.desiredVelocity.magnitude;

            // if desiredVelocity is tiny but sheep is still actually moving, use world position movement
            if (usePositionCheckBackup && worldMoveSpeed > movementValue)
            {
                movementValue = worldMoveSpeed;
            }
        }
        else
        {
            movementValue = worldMoveSpeed;
        }

        bool isMoving = movementValue > walkThreshold;

        animator.SetBool("IsWalking", isMoving);

        // Optional debug
        // Debug.Log("Movement Value: " + movementValue + " | IsMoving: " + isMoving);
    }
}