using UnityEngine;
using UnityEngine.AI;

public class BadSheepAI : MonoBehaviour
{
    public enum State
    {
        Wander, Idle, Stare, Follow,
        StandUp,
        WeirdFemaleDance, WeirdHeadSpin, WeirdHipHop, WeirdRunningCrawl, WeirdQuickSteps,
        SprintChase, AttackSwipe, GoDown,
        Escape
    }

    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Transform headPivot;
    public Transform eyePoint;

    [Header("Model Swap References")]
    public GameObject normalModel;
    public GameObject mutantModel;
    public Animator normalAnim;
    public Animator mutantAnim;
    private Animator currentAnim;

    [Header("Head / Eye Look")]
    public float eyeHeight = 0.6f;
    public float headTurnSpeed = 8f;
    public float headMaxYaw = 50f;
    public float headMaxPitch = 20f;
    public float minHeadLookDistance = 1.8f;
    public float noHeadLookDistance = 0.9f;
    public float veryCloseHeadReturnSpeed = 10f;

    [Header("Distances")]
    public float sightDistance = 10f;
    public float followDistance = 6f;
    public float loseInterestDistance = 14f;
    public float followStopDistance = 2.0f;
    public float followResumeDistance = 2.6f;
    public float playerTooCloseStopFollow = 1.2f;

    [Header("Line of Sight")]
    public bool requireLineOfSight = false;
    public LayerMask obstacleMask = ~0;

    [Header("Normal Behavior Timers")]
    public float wanderRadius = 10f;
    public float wanderSecondsMin = 2f;
    public float wanderSecondsMax = 5f;
    public float idleSecondsMin = 1f;
    public float idleSecondsMax = 3f;
    public float stareSecondsMin = 1f;
    public float stareSecondsMax = 2.5f;

    [Header("Follow Settings")]
    [Range(0f, 1f)]
    public float followChanceWhenClose = 0.15f;
    public float followSecondsMin = 1.5f;
    public float followSecondsMax = 3.0f;
    public float followRepathInterval = 0.25f;
    public float followCooldownSeconds = 3f;

    [Header("Body Turns")]
    public float normalBodyTurnSpeed = 10f;
    public float stareBodyTurnSpeed = 3f;
    public float followBodyTurnSpeed = 6f;
    public float bodyForwardYawOffset = 0f;

    [Header("Triggered Setup & Animation Timings")]
    public bool isTriggered = false;
    public float triggerAttackDistance = 8f;
    public float standUpDuration = 1.5f;
    public float goDownDuration = 1.0f;
    public float weirdBurstDuration = 3f;

    [Header("Attack Settings")]
    public float attackDuration = 4f;
    public float attackHitDistance = 1.5f;
    public float stunDuration = 0.5f;
    public float attackSwipeAnimDuration = 1.5f;
    public float bipedSprintSpeed = 4.5f;

    [Header("Escape Settings")]
    public float escapeDuration = 4f;
    public float escapeDistance = 20f;
    public float escapeSpeedMultiplier = 2.5f;
    public float escapeTurnSpeed = 12f;
    public float escapeSampleRadius = 10f;

    public State currentState = State.Wander;
    float stateTimer;
    float repathTimer;
    float followCooldownTimer;
    float defaultAgentSpeed;
    bool intendToAttackAfterStandUp = false;
    Quaternion headLocalDefaultRot;

    void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();

        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (headPivot) headLocalDefaultRot = headPivot.localRotation;

        defaultAgentSpeed = agent.speed;

        SwapToNormalModel();
        EnterState(State.Wander);
    }

    void Update()
    {
        if (currentAnim) currentAnim.SetFloat("Speed", agent.velocity.magnitude);

        if (!isTriggered)
            UpdateNormalSheep();
        else
            UpdateTriggeredSheep();
    }

    public void TriggerBadSheep()
    {
        if (isTriggered) return;
        isTriggered = true;

        float dist = Vector3.Distance(transform.position, player.position);
        intendToAttackAfterStandUp = (dist <= triggerAttackDistance);

        EnterState(State.StandUp);
    }

    void SwapToNormalModel()
    {
        if (mutantModel) mutantModel.SetActive(false);
        if (normalModel) normalModel.SetActive(true);
        currentAnim = normalAnim;
    }

    void SwapToMutantModel()
    {
        if (normalModel) normalModel.SetActive(false);
        if (mutantModel) mutantModel.SetActive(true);
        currentAnim = mutantAnim;
    }

    void UpdateNormalSheep()
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

        if (dist > loseInterestDistance && (currentState == State.Stare || currentState == State.Follow))
            EnterState(State.Wander);

        if (canSeePlayer && (currentState == State.Wander || currentState == State.Idle))
        {
            if (Random.value < 0.35f) EnterState(State.Stare);
        }

        if (followCooldownTimer <= 0f && dist <= followDistance && dist > followResumeDistance &&
            (currentState == State.Wander || currentState == State.Idle || currentState == State.Stare))
        {
            if (Random.value < (followChanceWhenClose * 2.0f) * Time.deltaTime)
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
        if (stateTimer <= 0f) EnterState(currentState == State.Wander ? State.Idle : State.Wander);
    }

    void TickNormalState()
    {
        stateTimer -= Time.deltaTime;
        float distToPlayer = player ? Vector3.Distance(transform.position, player.position) : Mathf.Infinity;

        switch (currentState)
        {
            case State.Wander:
            case State.Idle:
                if (stateTimer <= 0f) EnterState(currentState == State.Wander ? State.Idle : State.Wander);
                break;
            case State.Stare:
                if (stateTimer <= 0f) EnterState(Random.value < 0.5f ? State.Idle : State.Wander);
                break;
            case State.Follow:
                if (distToPlayer <= playerTooCloseStopFollow)
                {
                    followCooldownTimer = followCooldownSeconds;
                    EnterState(State.Idle);
                    return;
                }
                repathTimer -= Time.deltaTime;
                if (repathTimer <= 0f)
                {
                    repathTimer = followRepathInterval;
                    if (distToPlayer > followStopDistance) agent.SetDestination(player.position);
                    else agent.ResetPath();
                }
                if (stateTimer <= 0f)
                {
                    followCooldownTimer = followCooldownSeconds;
                    EnterState(Random.value < 0.6f ? State.Wander : State.Idle);
                }
                break;
        }
    }

    void UpdateTriggeredSheep()
    {
        stateTimer -= Time.deltaTime;
        float dist = player ? Vector3.Distance(transform.position, player.position) : 99f;

        ResetHeadToDefault();

        switch (currentState)
        {
            case State.StandUp:
                if (stateTimer <= 0f)
                {
                    if (intendToAttackAfterStandUp) EnterState(State.SprintChase);
                    else EnterRandomWeirdState();
                }
                break;
            case State.WeirdFemaleDance:
            case State.WeirdHeadSpin:
            case State.WeirdHipHop:
            case State.WeirdRunningCrawl:
            case State.WeirdQuickSteps:
                if (dist <= triggerAttackDistance)
                {
                    EnterState(State.SprintChase);
                    return;
                }
                if (stateTimer <= 0f) EnterState(State.GoDown);
                break;
            case State.SprintChase:
                FaceMovement(normalBodyTurnSpeed * 2f);
                if (player) agent.SetDestination(player.position);

                if (dist <= attackHitDistance)
                    EnterState(State.AttackSwipe);
                else if (stateTimer <= 0f)
                    EnterState(State.GoDown);
                break;
            case State.AttackSwipe:
                FaceBodyTowardPlayer(normalBodyTurnSpeed);
                if (stateTimer <= 0f) EnterState(State.GoDown);
                break;
            case State.GoDown:
                if (stateTimer <= 0f) EnterState(State.Escape);
                break;
            case State.Escape:
                FaceMovement(escapeTurnSpeed);
                if (!agent.hasPath || agent.remainingDistance <= 0.3f) SetEscapeDestination();
                if (stateTimer <= 0f)
                {
                    isTriggered = false;
                    EnterState(State.Idle);
                }
                break;
        }
    }

    void EnterRandomWeirdState()
    {
        State[] weirdStates = { State.WeirdFemaleDance, State.WeirdHeadSpin, State.WeirdHipHop, State.WeirdRunningCrawl, State.WeirdQuickSteps };
        EnterState(weirdStates[Random.Range(0, weirdStates.Length)]);
    }

    void EnterState(State next)
    {
        currentState = next;

        switch (currentState)
        {
            case State.Idle:
            case State.Stare:
                SwapToNormalModel();
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed;
                if (currentState == State.Idle) { stateTimer = Random.Range(idleSecondsMin, idleSecondsMax); agent.ResetPath(); }
                if (currentState == State.Stare) { stateTimer = Random.Range(stareSecondsMin, stareSecondsMax); agent.ResetPath(); }
                if (currentAnim) currentAnim.CrossFade("Locomotion", 0.2f); // RESTORED
                break;

            case State.Wander:
            case State.Follow:
                SwapToNormalModel();
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed;
                if (currentState == State.Wander) { stateTimer = Random.Range(wanderSecondsMin, wanderSecondsMax); SetRandomWanderDestination(); }
                if (currentState == State.Follow) { stateTimer = Random.Range(followSecondsMin, followSecondsMax); repathTimer = 0f; }
                if (currentAnim) currentAnim.CrossFade("Locomotion", 0.2f); // RESTORED
                break;

            case State.StandUp:
                SwapToNormalModel();
                agent.ResetPath();
                agent.isStopped = true;
                stateTimer = standUpDuration;
                if (currentAnim) currentAnim.CrossFade("Stand Up", 0.15f);
                break;

            case State.SprintChase:
                SwapToMutantModel();
                agent.isStopped = false;
                agent.speed = bipedSprintSpeed;
                stateTimer = attackDuration;
                if (currentAnim) currentAnim.CrossFade("Sprint", 0.2f);
                break;

            case State.AttackSwipe:
                SwapToMutantModel();
                agent.ResetPath();
                agent.isStopped = true;
                stateTimer = attackSwipeAnimDuration;
                TriggerPlayerStun();
                if (currentAnim) currentAnim.CrossFade("Mutant Swipe", 0.1f);
                break;

            case State.WeirdFemaleDance: SwapToMutantModel(); agent.isStopped = true; stateTimer = weirdBurstDuration; if (currentAnim) currentAnim.CrossFade("Female Dance Pose", 0.2f); break;
            case State.WeirdHeadSpin: SwapToMutantModel(); agent.isStopped = true; stateTimer = weirdBurstDuration; if (currentAnim) currentAnim.CrossFade("Head Spinning", 0.2f); break;
            case State.WeirdHipHop: SwapToMutantModel(); agent.isStopped = true; stateTimer = weirdBurstDuration; if (currentAnim) currentAnim.CrossFade("Hip Hop Dancing", 0.2f); break;
            case State.WeirdRunningCrawl: SwapToMutantModel(); agent.isStopped = true; stateTimer = weirdBurstDuration; if (currentAnim) currentAnim.CrossFade("Running Crawl", 0.2f); break;
            case State.WeirdQuickSteps: SwapToMutantModel(); agent.isStopped = true; stateTimer = weirdBurstDuration; if (currentAnim) currentAnim.CrossFade("Quick Steps", 0.2f); break;

            case State.GoDown:
                SwapToNormalModel();
                agent.ResetPath();
                agent.isStopped = true;
                stateTimer = goDownDuration;
                if (currentAnim) currentAnim.CrossFade("Go Down", 0.15f);
                break;

            case State.Escape:
                SwapToNormalModel();
                agent.isStopped = false;
                agent.speed = defaultAgentSpeed * escapeSpeedMultiplier;
                stateTimer = escapeDuration;
                SetEscapeDestination();
                if (currentAnim) currentAnim.CrossFade("Locomotion", 0.2f); // RESTORED
                break;
        }
    }

    void TriggerPlayerStun()
    {
        if (!player) return;

        SimplePlayerStun stun = player.GetComponent<SimplePlayerStun>();
        if (stun != null)
        {
            stun.ApplyStun(stunDuration);
            Debug.Log("Player Stunned!");
        }
    }

    void SetEscapeDestination()
    {
        Vector3 dir = player ? transform.position - player.position : transform.forward;
        dir.y = 0f;
        dir = dir.normalized + new Vector3(Random.Range(-0.35f, 0.35f), 0f, Random.Range(-0.35f, 0.35f));
        Vector3 rawTarget = transform.position + dir.normalized * escapeDistance;
        if (NavMesh.SamplePosition(rawTarget, out NavMeshHit hit, escapeSampleRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void SetRandomWanderDestination()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = transform.position + new Vector3(r.x, 0, r.y);
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.5f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
    }

    void FaceMovement(float turnSpd)
    {
        Vector3 v = agent.desiredVelocity; v.y = 0f;
        if (v.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v.normalized) * Quaternion.Euler(0, bodyForwardYawOffset, 0), Time.deltaTime * turnSpd);
    }

    void FaceBodyTowardPlayer(float turnSpd)
    {
        if (!player) return;
        Vector3 dir = player.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(0, bodyForwardYawOffset, 0), Time.deltaTime * turnSpd);
    }

    void AimHeadAtPlayer()
    {
        if (!headPivot || !player) return;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= noHeadLookDistance) { ResetHeadToDefault(); return; }
        Vector3 localDir = transform.InverseTransformDirection((player.position + Vector3.up * 0.8f - headPivot.position).normalized);
        float yaw = Mathf.Clamp(Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg, -headMaxYaw, headMaxYaw);
        float pitch = Mathf.Clamp(-Mathf.Asin(localDir.y) * Mathf.Rad2Deg, -headMaxPitch, headMaxPitch);

        Quaternion tgt = Quaternion.Euler(pitch, yaw, 0f);
        if (dist < minHeadLookDistance) tgt = Quaternion.Slerp(headLocalDefaultRot, tgt, Mathf.InverseLerp(noHeadLookDistance, minHeadLookDistance, dist));
        headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, tgt, Time.deltaTime * headTurnSpeed);
    }

    void ResetHeadToDefault()
    {
        if (headPivot) headPivot.localRotation = Quaternion.Slerp(headPivot.localRotation, headLocalDefaultRot, Time.deltaTime * headTurnSpeed);
    }

    bool HasLineOfSightToPlayer()
    {
        Vector3 from = eyePoint ? eyePoint.position : (transform.position + Vector3.up * eyeHeight);
        Vector3 to = player.position + Vector3.up * 0.8f;
        if (Physics.Raycast(from, (to - from).normalized, out RaycastHit hit, Vector3.Distance(from, to), obstacleMask))
            return hit.transform == player || hit.transform.IsChildOf(player);
        return true;
    }
}