using UnityEngine;
using UnityEngine.AI;

public class BadSheepAI : MonoBehaviour
{
    public enum State
    {
        Wander,
        Idle,
        Stare,
        Follow,

        WeirdDance,
        WeirdCrawl,
        WeirdFreezeLook,
        WeirdSpinRun,

        Attack,
        Escape
    }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Transform headPivot;
    public Transform eyePoint;
    public Transform visualRoot;

    [Header("Head / Eye Look")]
    public float eyeHeight = 0.6f;
    public float headTurnSpeed = 8f;
    public float headMaxYaw = 50f;
    public float headMaxPitch = 20f;

    [Header("Head Close-Range Fix")]
    public float minHeadLookDistance = 1.8f;
    public float noHeadLookDistance = 0.9f;
    public float veryCloseHeadReturnSpeed = 10f;

    [Header("Distances")]
    public float sightDistance = 10f;
    public float followDistance = 6f;
    public float loseInterestDistance = 14f;

    [Header("Follow Personal Space")]
    public float followStopDistance = 2.0f;
    public float followResumeDistance = 2.6f;
    public float playerTooCloseStopFollow = 1.2f;

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
    public float attackBodyTurnSpeed = 10f;
    public float bodyForwardYawOffset = 0f;

    [Header("Triggered Logic")]
    public bool isTriggered = false;
    public float triggerAttackDistance = 8f; // "x distance" for triggering attack

    [Header("Weird State")]
    public float weirdBurstDuration = 0.75f; // "x seconds" of weird state before escape
    public float weirdMoveRadius = 8f;

    [Header("Attack")]
    public float attackDuration = 3f; // Increased so it has time to chase the player
    public float stunDuration = 0.5f;
    public float attackHitDistance = 1.5f; // Must get this close to trigger stun
    public float attackSpeedMultiplier = 1.5f; // Little speed boost to catch the player

    [Header("Escape")]
    public float escapeDuration = 4f;
    public float escapeDistance = 20f;
    public float escapeSpeedMultiplier = 2.5f;
    public float escapeTurnSpeed = 12f;
    public float escapeSampleRadius = 10f;

    [Header("Weird State Visuals")]
    public float danceSpinSpeed = 200f;
    public float danceBobHeight = 0.3f;
    public float danceBobSpeed = 4f;
    public float crawlHeightOffset = -0.3f;
    public float spinRunSpinSpeed = 500f;

    [Header("Debug")]
    public State currentState = State.Wander;

    float stateTimer;
    float repathTimer;
    float followCooldownTimer;

    Quaternion headLocalDefaultRot;
    Vector3 visualDefaultPos;
    Quaternion visualDefaultRot;
    float defaultAgentSpeed;

    bool attackTriggeredThisState = false;

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

        if (visualRoot)
        {
            visualDefaultPos = visualRoot.localPosition;
            visualDefaultRot = visualRoot.localRotation;
        }

        defaultAgentSpeed = agent.speed;
        EnterState(State.Wander);
    }

    void Update()
    {
        if (!isTriggered)
            UpdateNormalSheep();
        else
            UpdateTriggeredSheep();
    }

    public void TriggerBadSheep()
    {
        if (isTriggered) return;
        if (!player) return;

        isTriggered = true;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= triggerAttackDistance)
        {
            // BRANCH 2: Player triggers sheep within X distance -> Attack (Chase) -> Stun -> Escape
            EnterState(State.Attack);
        }
        else
        {
            // BRANCH 1 & 3: Player triggers sheep out of X distance -> Weird State
            EnterRandomWeirdState();
        }
    }

    // ==================================================
    // NORMAL SHEEP LOGIC
    // ==================================================
    void UpdateNormalSheep()
    {
        RestoreVisual();
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
                EnterState(State.Stare);
        }

        if (followCooldownTimer <= 0f &&
            closeEnoughToConsiderFollow &&
            dist > followResumeDistance &&
            (currentState == State.Wander || currentState == State.Idle || currentState == State.Stare))
        {
            float chancePerSecond = followChanceWhenClose * 2.0f;
            if (Random.value < chancePerSecond * Time.deltaTime)
                EnterState(State.Follow);
        }

        TickNormalState();

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

    void TickNormalState()
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
                    EnterState(Random.value < 0.5f ? State.Idle : State.Wander);
                break;

            case State.Follow:
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

    // ==================================================
    // TRIGGERED LOGIC
    // ==================================================
    void UpdateTriggeredSheep()
    {
        // BRANCH 3: Interrupt weird state if player enters X distance
        if (IsWeirdState(currentState) && player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= triggerAttackDistance)
            {
                EnterState(State.Attack);
                return;
            }
        }

        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.WeirdDance:
                DoWeirdDance();
                break;

            case State.WeirdCrawl:
                DoWeirdCrawl();
                break;

            case State.WeirdFreezeLook:
                DoWeirdFreezeLook();
                break;

            case State.WeirdSpinRun:
                DoWeirdSpinRun();
                break;

            case State.Attack:
                DoAttack();
                // Immediately escape after stun is triggered
                if (attackTriggeredThisState)
                {
                    EnterState(State.Escape);
                }
                break;

            case State.Escape:
                DoEscape();
                break;
        }

        // Timer transitions
        if (stateTimer <= 0f)
        {
            switch (currentState)
            {
                case State.WeirdDance:
                case State.WeirdCrawl:
                case State.WeirdFreezeLook:
                case State.WeirdSpinRun:
                    // BRANCH 1: Weird state ends, sheep escapes
                    EnterState(State.Escape);
                    break;

                case State.Attack:
                    // If the sheep chases for the whole duration and can't catch the player, it gives up and escapes
                    EnterState(State.Escape);
                    break;

                case State.Escape:
                    isTriggered = false;
                    EnterState(State.Idle);
                    break;
            }
        }
    }

    bool IsWeirdState(State s)
    {
        return s == State.WeirdDance ||
               s == State.WeirdCrawl ||
               s == State.WeirdFreezeLook ||
               s == State.WeirdSpinRun;
    }

    void EnterRandomWeirdState()
    {
        int r = Random.Range(0, 4);

        if (r == 0) EnterState(State.WeirdDance);
        else if (r == 1) EnterState(State.WeirdCrawl);
        else if (r == 2) EnterState(State.WeirdFreezeLook);
        else EnterState(State.WeirdSpinRun);
    }

    // ==================================================
    // BAD STATES
    // ==================================================
    void DoAttack()
    {
        if (!player) return;

        // Actively move towards the player
        agent.SetDestination(player.position);

        AimHeadAtPlayer();
        FaceBodyTowardPlayer(attackBodyTurnSpeed);

        float dist = Vector3.Distance(transform.position, player.position);

        // Only trigger stun if we physically reach the player
        if (dist <= attackHitDistance && !attackTriggeredThisState)
        {
            TriggerPlayerStun();
            attackTriggeredThisState = true;
        }

        if (visualRoot && !attackTriggeredThisState)
        {
            // Optional: Keeps the visual lunge running while it chases
            float t = 1f - (stateTimer / Mathf.Max(attackDuration, 0.01f));
            float lunge = Mathf.Sin(t * Mathf.PI * 4f) * 0.15f;
            visualRoot.localPosition = visualDefaultPos + new Vector3(0f, 0f, lunge);
        }
    }

    void DoEscape()
    {
        RestoreVisual();
        ResetHeadToDefault();

        if (!agent.hasPath || agent.remainingDistance <= 0.3f)
        {
            SetEscapeDestination();
        }

        agent.isStopped = false;
        agent.speed = defaultAgentSpeed * escapeSpeedMultiplier;
        FaceMovement(escapeTurnSpeed);
    }

    void DoWeirdDance()
    {
        agent.ResetPath();

        if (visualRoot)
        {
            float bob = Mathf.Sin(Time.time * danceBobSpeed) * danceBobHeight;
            visualRoot.localPosition = visualDefaultPos + new Vector3(0f, bob, 0f);
            visualRoot.Rotate(Vector3.up, danceSpinSpeed * Time.deltaTime, Space.Self);
        }
    }

    void DoWeirdCrawl()
    {
        agent.ResetPath();

        if (visualRoot)
        {
            visualRoot.localPosition = Vector3.Lerp(
                visualRoot.localPosition,
                visualDefaultPos + new Vector3(0f, crawlHeightOffset, 0f),
                Time.deltaTime * 8f
            );
        }
    }

    void DoWeirdFreezeLook()
    {
        agent.ResetPath();
    }

    void DoWeirdSpinRun()
    {
        agent.ResetPath();

        if (visualRoot)
        {
            visualRoot.Rotate(Vector3.up, spinRunSpinSpeed * Time.deltaTime, Space.Self);
        }
    }

    // ==================================================
    // STATE ENTRY
    // ==================================================
    void EnterState(State next)
    {
        currentState = next;
        attackTriggeredThisState = false;

        switch (currentState)
        {
            case State.Wander:
                RestoreVisual(true);
                stateTimer = Random.Range(wanderSecondsMin, wanderSecondsMax);
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed;
                agent.stoppingDistance = 0f;
                SetRandomWanderDestination();
                break;

            case State.Idle:
                RestoreVisual(true);
                stateTimer = Random.Range(idleSecondsMin, idleSecondsMax);
                agent.isStopped = false;
                agent.ResetPath();
                break;

            case State.Stare:
                RestoreVisual(false);
                stateTimer = Random.Range(stareSecondsMin, stareSecondsMax);
                agent.isStopped = false;
                agent.ResetPath();
                break;

            case State.Follow:
                RestoreVisual(false);
                stateTimer = Random.Range(followSecondsMin, followSecondsMax);
                repathTimer = 0f;
                agent.isStopped = false;
                agent.stoppingDistance = followStopDistance;
                break;

            case State.WeirdDance:
            case State.WeirdCrawl:
            case State.WeirdFreezeLook:
            case State.WeirdSpinRun:
                stateTimer = weirdBurstDuration;
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed;
                agent.stoppingDistance = 0f;
                agent.ResetPath();
                break;

            case State.Attack:
                stateTimer = attackDuration; // Gives sheep time to chase
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed * attackSpeedMultiplier; // Run to catch player
                agent.stoppingDistance = 0f;
                // Don't reset path here, it gets set in DoAttack() continuously
                break;

            case State.Escape:
                RestoreVisual(false);
                stateTimer = escapeDuration;
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed * escapeSpeedMultiplier;
                agent.stoppingDistance = 0f;
                SetEscapeDestination(); // immediate escape target
                break;
        }
    }

    // ==================================================
    // HELPERS
    // ==================================================
    void TriggerPlayerStun()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackHitDistance) return;

        SimplePlayerStun stun = player.GetComponent<SimplePlayerStun>();
        if (stun != null)
        {
            stun.ApplyStun(stunDuration);
        }
    }

    void SetEscapeDestination()
    {
        Vector3 dir;

        if (player)
        {
            dir = transform.position - player.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Random.insideUnitSphere;
                dir.y = 0f;
            }
        }
        else
        {
            dir = transform.forward;
        }

        dir = dir.normalized;
        dir += new Vector3(Random.Range(-0.35f, 0.35f), 0f, Random.Range(-0.35f, 0.35f));
        dir.Normalize();

        Vector3 rawTarget = transform.position + dir * escapeDistance;

        if (NavMesh.SamplePosition(rawTarget, out NavMeshHit hit, escapeSampleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // fallback: short hop somewhere valid
            Vector3 fallback = transform.position + dir * 5f;
            if (NavMesh.SamplePosition(fallback, out NavMeshHit hit2, escapeSampleRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit2.position);
            }
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
            return hit.transform == player || hit.transform.IsChildOf(player);

        return true;
    }

    void RestoreVisual(bool instant = false)
    {
        if (!visualRoot) return;

        if (instant)
        {
            visualRoot.localPosition = visualDefaultPos;
            visualRoot.localRotation = visualDefaultRot;
            return;
        }

        visualRoot.localPosition = Vector3.Lerp(
            visualRoot.localPosition,
            visualDefaultPos,
            Time.deltaTime * 6f
        );

        visualRoot.localRotation = Quaternion.Slerp(
            visualRoot.localRotation,
            visualDefaultRot,
            Time.deltaTime * 6f
        );
    }
}