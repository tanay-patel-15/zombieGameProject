using System;
using UnityEngine;
using UnityEngine.AI;

public enum InfectedState { Idle, Patrol, Chase, Attack }

// One infected: a small state machine on a NavMeshAgent. It waits at each waypoint, walks
// to the next, chases the player once it sees them and catches them in reach. It drives
// the Animator's Speed and Attack; the model is a child, so either zombie look fits.
[RequireComponent(typeof(NavMeshAgent))]
public class InfectedAI : MonoBehaviour
{
    // Fired once when the player is caught. GameOverUI shows the caught panel.
    public static event Action PlayerCaught;

    // The infected that's catching the player. The others stand down when it's set.
    static InfectedAI attacker;

    [Header("Patrol")]
    [SerializeField] Transform[] waypoints;
    [SerializeField] float patrolSpeed = 1.5f;
    [Tooltip("Seconds it waits at each waypoint.")]
    [SerializeField] float idleTime = 2f;

    [Header("Chase")]
    [Tooltip("Faster than the player walks (3.5), slower than they sprint (6.5).")]
    [SerializeField] float chaseSpeed = 4.5f;
    [Tooltip("Seconds without seeing the player before it gives up and goes back to patrolling.")]
    [SerializeField] float loseSightTime = 4f;
    [SerializeField] float repathInterval = 0.2f;

    [Header("Senses")]
    [SerializeField] float sightRange = 12f;
    [Tooltip("Full width of the view cone (degrees).")]
    [SerializeField] float sightAngle = 100f;
    [Tooltip("Within this distance it notices the player whichever way it faces.")]
    [SerializeField] float closeRange = 2.5f;
    [SerializeField] float eyeHeight = 1.6f;
    [Tooltip("Height on the player it looks for (their chest).")]
    [SerializeField] float targetHeight = 1.2f;
    [Tooltip("Layers that block its sight: walls, shelves, closed doors.")]
    [SerializeField] LayerMask obstructionMask = 1; // Default

    [Header("Attack")]
    [SerializeField] float attackRange = 1.4f;
    [Tooltip("Seconds from the start of the attack to the player being caught.")]
    [SerializeField] float catchDelay = 0.8f;
    [SerializeField] float turnSpeed = 540f;

    static readonly int SpeedParam = Animator.StringToHash("Speed");
    static readonly int AttackParam = Animator.StringToHash("Attack");

    NavMeshAgent agent;
    Animator animator;
    Transform player;
    PlayerController playerController;
    int waypointIndex = -1;
    float stateStartTime;
    float lastSeenTime = float.NegativeInfinity;
    Vector3 lastSeenPosition;
    float nextRepathTime;
    bool caught;

    public InfectedState State { get; private set; }

    // Domain reload is off in this project, so statics survive between play sessions.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        PlayerCaught = null;
        attacker = null;
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("InfectedAI: no object tagged Player in the scene.", this);
        }

        EnterIdle();
    }

    void OnDestroy()
    {
        if (attacker == this)
            attacker = null;
    }

    void Update()
    {
        if (attacker != null && attacker != this)
        {
            StandDown();
            return;
        }

        bool seesPlayer = player != null && CanSeePlayer();
        if (seesPlayer)
        {
            lastSeenTime = Time.time;
            lastSeenPosition = player.position;
        }

        switch (State)
        {
            case InfectedState.Idle:
                if (seesPlayer)
                    EnterChase();
                else if (waypoints.Length > 0 && Time.time - stateStartTime >= idleTime)
                    EnterPatrol();
                break;

            case InfectedState.Patrol:
                if (seesPlayer)
                    EnterChase();
                else if (ReachedDestination())
                    EnterIdle();
                break;

            case InfectedState.Chase:
                UpdateChase(seesPlayer);
                break;

            case InfectedState.Attack:
                UpdateAttack();
                break;
        }

        if (animator != null)
            animator.SetFloat(SpeedParam, agent.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    void EnterIdle()
    {
        SetState(InfectedState.Idle);
        if (agent.isOnNavMesh)
            agent.ResetPath();
    }

    void EnterPatrol()
    {
        SetState(InfectedState.Patrol);
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.3f;
        if (agent.isOnNavMesh && waypoints[waypointIndex] != null)
            agent.SetDestination(waypoints[waypointIndex].position);
    }

    void EnterChase()
    {
        SetState(InfectedState.Chase);
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 1f;
        nextRepathTime = 0f;
    }

    // While it can see the player it runs at them. Once it loses them it goes to where
    // they were last seen, and gives up if they don't show again in time. A player in UV
    // light is never seen, so stepping into one ends the chase at once.
    void UpdateChase(bool seesPlayer)
    {
        if (UVSafeZone.IsSafe(player.position) || (!seesPlayer && Time.time - lastSeenTime >= loseSightTime))
        {
            EnterIdle();
            return;
        }

        if (seesPlayer && InAttackRange())
        {
            EnterAttack();
            return;
        }

        if (Time.time >= nextRepathTime && agent.isOnNavMesh)
        {
            agent.SetDestination(seesPlayer ? player.position : lastSeenPosition);
            nextRepathTime = Time.time + repathInterval;
        }
    }

    void EnterAttack()
    {
        SetState(InfectedState.Attack);
        attacker = this;
        if (agent.isOnNavMesh)
            agent.ResetPath();
        agent.velocity = Vector3.zero;

        if (animator != null)
            animator.SetTrigger(AttackParam);
        if (playerController != null)
            playerController.SetInputLocked(true);
    }

    void UpdateAttack()
    {
        FacePlayer();
        if (!caught && Time.time - stateStartTime >= catchDelay)
        {
            caught = true;
            PlayerCaught?.Invoke();
        }
    }

    // Another infected is catching the player: stop where it is.
    void StandDown()
    {
        if (agent.isOnNavMesh)
            agent.ResetPath();
        agent.velocity = Vector3.zero;
        if (animator != null)
            animator.SetFloat(SpeedParam, 0f);
        enabled = false;
    }

    void SetState(InfectedState state)
    {
        State = state;
        stateStartTime = Time.time;
    }

    bool CanSeePlayer()
    {
        if (UVSafeZone.IsSafe(player.position))
            return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 toPlayer = target - eye;
        float distance = toPlayer.magnitude;
        if (distance > sightRange)
            return false;

        bool inCone = Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(toPlayer, Vector3.up)) <= sightAngle * 0.5f;
        if (!inCone && distance > closeRange)
            return false;

        return !Physics.Linecast(eye, target, obstructionMask, QueryTriggerInteraction.Ignore);
    }

    bool InAttackRange()
    {
        Vector3 offset = player.position - transform.position;
        return Mathf.Abs(offset.y) < 1.5f && Vector3.ProjectOnPlane(offset, Vector3.up).magnitude <= attackRange;
    }

    bool ReachedDestination()
    {
        if (!agent.isOnNavMesh || agent.pathPending)
            return false;
        return agent.remainingDistance <= agent.stoppingDistance + 0.1f;
    }

    void FacePlayer()
    {
        Vector3 flat = Vector3.ProjectOnPlane(player.position - transform.position, Vector3.up);
        if (flat.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat), turnSpeed * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Gizmos.color = new Color(1f, 0.3f, 0.2f);
        Vector3 left = Quaternion.Euler(0f, -sightAngle * 0.5f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, sightAngle * 0.5f, 0f) * transform.forward;
        Gizmos.DrawLine(eye, eye + left * sightRange);
        Gizmos.DrawLine(eye, eye + right * sightRange);
        Gizmos.DrawWireSphere(transform.position, closeRange);

        if (waypoints == null)
            return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null || waypoints[(i + 1) % waypoints.Length] == null)
                continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            Gizmos.DrawLine(waypoints[i].position, waypoints[(i + 1) % waypoints.Length].position);
        }
    }
}
