using UnityEngine;
using UnityEngine.AI;

public class SheepAI : MonoBehaviour
{
    public enum State { Wander, Idle, Stare, Follow }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;

    [Header("Visual Aim (Head)")]
    public Transform headPivot;                 // Drag HeadPivot here
    public float headTurnSpeed = 8f;
    public float headMaxYaw = 60f;              // left/right clamp
    public float headMaxPitch = 25f;            // up/down clamp
    public float eyeHeight = 0.6f;              // if you don’t have eyePoint
    public Transform eyePoint;                  // optional

    [Header("Distances")]
    public float sightDistance = 10f;
    public float followDistance = 6f;
    public float loseInterestDistance = 14f;

    [Header("Line of Sight")]
    public bool requireLineOfSight = false;
    public LayerMask obstacleMask = ~0;

    [Header("Wander")]
    public float wanderRadius = 10f;
    public float wanderSecondsMin = 2f;
    public float wanderSecondsMax = 5f;

    [Header("Idle")]
    public float idleSecondsMin = 1f;
    public float idleSecondsMax = 3f;

    [Header("Stare")]
    public float stareSecondsMin = 0.8f;
    public float stareSecondsMax = 2.0f;

    [Header("Follow")]
    [Range(0f, 1f)]
    public float followChanceWhenClose = 0.25f;
    public float followSecondsMin = 1.5f;
    public float followSecondsMax = 4.0f;
    public float followRepathInterval = 0.25f;

    [Header("Anti-Spam Follow")]
    public float followCooldownSeconds = 2.5f;   // prevents immediate re-follow

    [Header("Body Facing")]
    public float bodyTurnSpeed = 10f;            // faces movement direction

    [Header("Debug")]
    public State currentState = State.Wander;

    float stateTimer;
    float repathTimer;
    float followCooldownTimer;

    Quaternion headLocalDefaultRot;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (headPivot)
            headLocalDefaultRot = headPivot.localRotation;

        EnterState(State.Wander);
    }

    void Update()
    {
        followCooldownTimer -= Time.deltaTime;

        if (!player)
        {
            TickState_NoPlayer();
            FaceMovement();         // still face movement if wandering
            ResetHeadToDefault();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = dist <= sightDistance && (!requireLineOfSight || HasLineOfSightToPlayer());
        bool closeEnoughToConsiderFollow = dist <= followDistance;

        // If player is far, drop special states
        if (dist > loseInterestDistance && (currentState == State.Stare || currentState == State.Follow))
        {
            EnterState(State.Wander);
        }

        // Random stare trigger (when noticing player)
        if (canSeePlayer && (currentState == State.Wander || currentState == State.Idle))
        {
            if (Random.value < 0.35f)
                EnterState(State.Stare);
        }

        // Random follow trigger (only if cooldown is over)
        if (followCooldownTimer <= 0f &&
            closeEnoughToConsiderFollow &&
            (currentState == State.Wander || currentState == State.Idle || currentState == State.Stare))
        {
            // scaled by deltaTime so it’s "sometimes", not constant
            float chancePerSecond = followChanceWhenClose * 2.5f;
            if (Random.value < chancePerSecond * Time.deltaTime)
                EnterState(State.Follow);
        }

        TickState(dist);

        // Rotation behavior:
        // - During stare: body can stay put; head tracks player
        // - Otherwise: body faces movement direction; head resets (or you can make it mild look)
        if (currentState == State.Stare || currentState == State.Follow)
        {
            AimHeadAtPlayer();
        }
        else
        {
            ResetHeadToDefault();
        }

        FaceMovement();
    }

    void TickState_NoPlayer()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            if (currentState == State.Wander) EnterState(State.Idle);
            else EnterState(State.Wander);
        }
    }

    void TickState(float distToPlayer)
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Wander:
                if (stateTimer <= 0f)
                    EnterState(State.Idle);
                break;

            case State.Idle:
                if (stateTimer <= 0f)
                    EnterState(State.Wander);
                break;

            case State.Stare:
                agent.ResetPath(); // stop moving
                if (stateTimer <= 0f)
                    EnterState(Random.value < 0.5f ? State.Idle : State.Wander);
                break;

            case State.Follow:
                // Follow only for a few seconds, then stop.
                repathTimer -= Time.deltaTime;
                if (repathTimer <= 0f)
                {
                    repathTimer = followRepathInterval;
                    agent.SetDestination(player.position);
                }

                if (stateTimer <= 0f)
                {
                    // STOP following (important)
                    agent.ResetPath();

                    // cooldown so it won’t instantly follow again
                    followCooldownTimer = followCooldownSeconds;

                    // go back to normal life
                    EnterState(Random.value < 0.6f ? State.Wander : State.Idle);
                }
                break;
        }
    }

    void EnterState(State next)
    {
        currentState = next;

        switch (currentState)
        {
            case State.Wander:
                stateTimer = Random.Range(wanderSecondsMin, wanderSecondsMax);
                agent.isStopped = false;
                SetRandomWanderDestination();
                break;

            case State.Idle:
                stateTimer = Random.Range(idleSecondsMin, idleSecondsMax);
                agent.ResetPath();
                break;

            case State.Stare:
                stateTimer = Random.Range(stareSecondsMin, stareSecondsMax);
                agent.ResetPath();
                break;

            case State.Follow:
                stateTimer = Random.Range(followSecondsMin, followSecondsMax);
                repathTimer = 0f;
                agent.isStopped = false;
                break;
        }
    }

    void SetRandomWanderDestination()
    {
        Vector3 origin = transform.position;
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = new Vector3(origin.x + r.x, origin.y, origin.z + r.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // --- BODY FACING FIX (moves where it's pointing) ---
    void FaceMovement()
    {
        // If not moving, don’t rotate body.
        Vector3 vel = agent.velocity;
        vel.y = 0f;
        if (vel.sqrMagnitude < 0.02f) return;

        // Face the direction you're actually moving
        Quaternion targetRot = Quaternion.LookRotation(vel.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * bodyTurnSpeed);
    }

    // --- HEAD/ EYELINE FIX ---
    void AimHeadAtPlayer()
    {
        if (!headPivot || !player) return;

        Vector3 from = headPivot.position;
        Vector3 to = player.position + Vector3.up * 0.8f;
        Vector3 dir = (to - from).normalized;

        // Convert world direction into local space of the sheep root
        Vector3 localDir = transform.InverseTransformDirection(dir);

        // yaw (left/right) and pitch (up/down)
        float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(localDir.y) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -headMaxYaw, headMaxYaw);
        pitch = Mathf.Clamp(pitch, -headMaxPitch, headMaxPitch);

        Quaternion targetLocal = Quaternion.Euler(pitch, yaw, 0f);
        headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, targetLocal, Time.deltaTime * headTurnSpeed);
    }

    void ResetHeadToDefault()
    {
        if (!headPivot) return;
        headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, headLocalDefaultRot, Time.deltaTime * headTurnSpeed);
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 from = eyePoint ? eyePoint.position : (transform.position + Vector3.up * eyeHeight);
        Vector3 to = player.position + Vector3.up * 0.8f;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);

        if (Physics.Raycast(from, dir, out RaycastHit hit, dist, obstacleMask))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }
        return true;
    }
}