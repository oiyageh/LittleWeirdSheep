using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI; // Optional: For AI navigation

[RequireComponent(typeof(Animator))]
public class AIAnimationController : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool useNavMeshAgent = true; // Toggle between AI and manual movement
    public float walkThreshold = 0.1f;  // Speed at which walking starts
    public float runThreshold = 2.0f;   // Speed at which running starts

    private Animator animator;
    private NavMeshAgent agent; // For AI movement
    private Vector3 lastPosition;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (useNavMeshAgent)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError("NavMeshAgent not found. Please add one or disable 'useNavMeshAgent'.");
            }
        }
    }

    void Update()
    {
        float speed = 0f;

        if (useNavMeshAgent && agent != null)
        {
            // AI movement speed from NavMeshAgent
            speed = agent.velocity.magnitude;
        }
        else
        {
            // Manual movement speed calculation
            speed = ((transform.position - lastPosition) / Time.deltaTime).magnitude;
            lastPosition = transform.position;
        }

        // Set Animator parameters
        animator.SetFloat("Speed", speed);

        // Optional: Boolean states for blending
        animator.SetBool("IsWalking", speed > walkThreshold && speed <= runThreshold);
        animator.SetBool("IsRunning", speed > runThreshold);
    }
}
