using UnityEngine;
using UnityEngine.AI;

public class SheepAI : MonoBehaviour
{
    public enum State { Wander, Idle, Stare, Follow }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;

    [Header("Head / Eye Look")]
    public Transform headPivot;
    public Transform eyePoint;
    public float eyeHeight = 0.6f;
    public float headTurnSpeed = 8f;
    public float headMaxYaw = 50f;
    public float headMaxPitch = 20f;

    [Header("Head Close-Range Fix")]
    public float minHeadLookDistance = 1.8f;       // if player is closer than this, head stops trying to fully aim
    public float noHeadLookDistance = 0.9f;        // if player is closer than this, head returns to default
    public float veryCloseHeadReturnSpeed = 10f;

    [Header("Distances")]
    public float sightDistance = 10f;
    public float followDistance = 6f;
    public float loseInterestDistance = 14f;

    [Header("Follow Personal Space")]
    public float followStopDistance = 2.0f;        // sheep stops this far away from player
    public float followResumeDistance = 2.6f;      // must be at least this far to continue chasing
    public float playerTooCloseStopFollow = 1.2f;  // if closer than this, stop following immediately

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
    public float stareSecondsMin = 1f;
    public float stareSecondsMax = 2.5f;

    [Header("Follow")]
    [Range(0f, 1f)]
    public float followChanceWhenClose = 0.15f;
    public float followSecondsMin = 1.5f;
    public float followSecondsMax = 3.0f;
    public float followRepathInterval = 0.25f;
    public float followCooldownSeconds = 3f;

    [Header("Body Turn")]
    public float normalBodyTurnSpeed = 10f;
    public float stareBodyTurnSpeed = 3f;
    public float followBodyTurnSpeed = 6f;
    public float bodyForwardYawOffset = 0f;

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
            TickStateNoPlayer();
            FaceMovement(normalBodyTurnSpeed);
            ResetHeadToDefault();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        bool canSeePlayer = dist <= sightDistance && (!requireLineOfSight || HasLineOfSightToPlayer());
        bool closeEnoughToConsiderFollow = dist <= followDistance;

        if (dist > loseInterestDistance && (currentState == State.Stare || currentState == State.Follow))
        {
            EnterState(State.Wander);
        }

        if (canSeePlayer && (currentState == State.Wander || currentState == State.Idle))
        {
            if (Random.value < 0.35f)
            {
                EnterState(State.Stare);
            }
        }

        if (followCooldownTimer <= 0f &&
            closeEnoughToConsiderFollow &&
            dist > followResumeDistance &&
            (currentState == State.Wander || currentState == State.Idle || currentState == State.Stare))
        {
            float chancePerSecond = followChanceWhenClose * 2.0f;
            if (Random.value < chancePerSecond * Time.deltaTime)
            {
                EnterState(State.Follow);
            }
        }

        TickState();

        if (currentState == State.Stare)
        {
            AimHeadAtPlayer();
            FaceBodyTowardPlayer(stareBodyTurnSpeed);
        }
        else if (currentState == State.Follow)
        {
            AimHeadAtPlayer();
            FaceBodyTowardPlayer(followBodyTurnSpeed);
        }
        else
        {
            ResetHeadToDefault();
            FaceMovement(normalBodyTurnSpeed);
        }
    }

    void TickStateNoPlayer()
    {
        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            if (currentState == State.Wander) EnterState(State.Idle);
            else EnterState(State.Wander);
        }
    }

    void TickState()
    {
        stateTimer -= Time.deltaTime;

        float distToPlayer = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

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
                agent.ResetPath();

                if (stateTimer <= 0f)
                {
                    EnterState(Random.value < 0.5f ? State.Idle : State.Wander);
                }
                break;

            case State.Follow:
                // Stop following if player is already too close
                if (distToPlayer <= playerTooCloseStopFollow)
                {
                    agent.ResetPath();
                    followCooldownTimer = followCooldownSeconds;
                    EnterState(State.Idle);
                    return;
                }

                repathTimer -= Time.deltaTime;

                if (repathTimer <= 0f)
                {
                    repathTimer = followRepathInterval;

                    // Only chase if player is still outside personal-space distance
                    if (distToPlayer > followStopDistance)
                    {
                        Vector3 dir = (transform.position - player.position).normalized;
                        Vector3 targetPos = player.position + dir * followStopDistance;

                        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                            agent.SetDestination(hit.position);
                        else
                            agent.SetDestination(player.position);
                    }
                    else
                    {
                        agent.ResetPath();
                    }
                }

                if (stateTimer <= 0f)
                {
                    agent.ResetPath();
                    followCooldownTimer = followCooldownSeconds;
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
                agent.stoppingDistance = 0f;
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
                agent.stoppingDistance = followStopDistance;
                break;
        }
    }

    void SetRandomWanderDestination()
    {
        Vector3 origin = transform.position;
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = new Vector3(origin.x + r.x, origin.y, origin.z + r.y);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void FaceMovement(float turnSpeed)
    {
        Vector3 v = agent.desiredVelocity;
        v.y = 0f;

        if (v.sqrMagnitude < 0.001f) return;

        Quaternion target = Quaternion.LookRotation(v.normalized);
        target *= Quaternion.Euler(0f, bodyForwardYawOffset, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * turnSpeed);
    }

    void FaceBodyTowardPlayer(float turnSpeed)
    {
        if (!player) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        targetRot *= Quaternion.Euler(0f, bodyForwardYawOffset, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
    }

    void AimHeadAtPlayer()
    {
        if (!headPivot || !player) return;

        Vector3 from = headPivot.position;
        Vector3 to = player.position + Vector3.up * 0.8f;

        float dist = Vector3.Distance(transform.position, player.position);

        // Extremely close: just return to default head pose
        if (dist <= noHeadLookDistance)
        {
            headPivot.localRotation = Quaternion.Slerp(
                headPivot.localRotation,
                headLocalDefaultRot,
                Time.deltaTime * veryCloseHeadReturnSpeed
            );
            return;
        }

        Quaternion targetLocal = GetHeadLookRotation(from, to);

        // Somewhat close: blend between default and look rotation
        if (dist < minHeadLookDistance)
        {
            float t = Mathf.InverseLerp(noHeadLookDistance, minHeadLookDistance, dist);
            targetLocal = Quaternion.Slerp(headLocalDefaultRot, targetLocal, t);
        }

        headPivot.localRotation = Quaternion.Slerp(
            headPivot.localRotation,
            targetLocal,
            Time.deltaTime * headTurnSpeed
        );
    }

    Quaternion GetHeadLookRotation(Vector3 from, Vector3 to)
    {
        Vector3 dir = (to - from).normalized;
        Vector3 localDir = transform.InverseTransformDirection(dir);

        float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(localDir.y) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -headMaxYaw, headMaxYaw);
        pitch = Mathf.Clamp(pitch, -headMaxPitch, headMaxPitch);

        return Quaternion.Euler(pitch, yaw, 0f);
    }

    void ResetHeadToDefault()
    {
        if (!headPivot) return;

        headPivot.localRotation = Quaternion.Slerp(
            headPivot.localRotation,
            headLocalDefaultRot,
            Time.deltaTime * headTurnSpeed
        );
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