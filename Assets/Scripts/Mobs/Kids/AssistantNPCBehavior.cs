using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class AssistantNPCBehavior : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique ID for this kid. Must be different for every kid. Set it on the prefab.")]
    [SerializeField] private string kidID;

    private static readonly Dictionary<string, AssistantNPCBehavior> activeKids =
        new Dictionary<string, AssistantNPCBehavior>();

    [Header("Movement")]
    [Tooltip("How fast the kid moves, in Unity units per second.")]
    [SerializeField] private float movementSpeed = 1.5f;

    [Tooltip("How close the kid must get to a destination before considering it reached.")]
    [SerializeField] private float arrivalTolerance = 0.2f;

    [Header("Following")]
    [Tooltip("Preferred position relative to the player. Different offsets help kids spread out.")]
    [SerializeField] private Vector2 followOffset = new Vector2(1.5f, 0f);

    [Tooltip("How far the kid can randomly roam around its preferred following position.")]
    [SerializeField] private float followingWanderRadius = 0.8f;

    [Tooltip("Minimum distance between the player and a newly selected following destination.")]
    [SerializeField] private float minimumPlayerDistance = 1.2f;

    [Tooltip("How far the player must move before the kid chooses a new following destination.")]
    [SerializeField] private float followRefreshDistance = 1f;

    [Tooltip("If the kid gets farther from the player than this distance, it chooses a new destination nearby.")]
    [SerializeField] private float maximumFollowDistance = 4f;

    [Tooltip("Shortest time the kid waits before choosing another following destination.")]
    [SerializeField] private float minWanderWait = 1f;

    [Tooltip("Longest time the kid waits before choosing another following destination.")]
    [SerializeField] private float maxWanderWait = 3f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Layers containing objects the kid should try to avoid. Exclude the player and the kid itself.")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Distance checked ahead of the kid to detect obstacles.")]
    [SerializeField] private float obstacleCheckDistance = 0.8f;

    [Tooltip("How long the kid continues moving in a chosen detour direction before checking its route again.")]
    [SerializeField] private float avoidanceDuration = 0.5f;

    [Header("Stuck Recovery")]
    [Tooltip("How often, in seconds, the script checks whether the kid has made progress.")]
    [SerializeField] private float stuckCheckInterval = 0.5f;

    [Tooltip("Minimum distance the kid must move between stuck checks to count as making progress.")]
    [SerializeField] private float minimumProgressDistance = 0.05f;

    [Tooltip("How long the kid can make too little progress before choosing a new destination.")]
    [SerializeField] private float maxStuckTime = 1.5f;

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 wanderTarget;
    private Vector2 followPosition;
    private Vector2 targetPlayerPosition;

    private Vector2 lastStuckCheckPosition;
    private Vector2 avoidanceDirection;

    private float wanderWaitTimer;
    private float stuckCheckTimer;
    private float stuckTimer;
    private float avoidanceTimer;

    private bool isWaiting;
    private bool hasWanderTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (string.IsNullOrEmpty(kidID))
            return;

        if (activeKids.TryGetValue(kidID, out AssistantNPCBehavior existing) &&
            existing != null &&
            existing != this)
        {
            // The original kid is still alive, so this is a restored duplicate.
            enabled = false;
            Destroy(gameObject);
            return;
        }

        activeKids[kidID] = this;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(kidID) &&
            activeKids.TryGetValue(kidID, out AssistantNPCBehavior existing) &&
            existing == this)
        {
            activeKids.Remove(kidID);
        }
    }

    private void Start()
    {
        FindPlayer();

        lastStuckCheckPosition = rb.position;

        if (player != null)
            StartFollowing((Vector2)player.position + followOffset);
        else
            StopMovement();
    }

    private void Update()
    {
        if (player == null)
            FindPlayer();

        if (player == null)
        {
            StopMovement();
            return;
        }

        UpdateFollowing();

        if (isWaiting)
        {
            wanderWaitTimer -= Time.deltaTime;

            if (wanderWaitTimer <= 0f)
                ChooseFollowingTarget();

            return;
        }

        if (hasWanderTarget)
            CheckStuck();
    }

    private void FixedUpdate()
    {
        if (isWaiting || !hasWanderTarget || player == null)
        {
            StopMovement();
            return;
        }

        MoveTowards(wanderTarget);
    }

    // PLAYER

    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    // FOLLOWING

    public void StartFollowing(Vector2 initialFollowPosition)
    {
        if (player == null)
            FindPlayer();

        if (player == null)
        {
            StopMovement();
            return;
        }

        followOffset = initialFollowPosition - (Vector2)player.position;

        isWaiting = false;
        hasWanderTarget = false;

        ResetMovementRecovery();
        ChooseFollowingTarget();
    }

    public void SetFollowPosition(Vector2 newFollowPosition)
    {
        if (player == null)
            FindPlayer();

        if (player != null)
            followOffset = newFollowPosition - (Vector2)player.position;
    }

    private void UpdateFollowing()
    {
        followPosition = (Vector2)player.position + followOffset;

        float playerMovement = Vector2.Distance(
            (Vector2)player.position,
            targetPlayerPosition
        );

        float playerDistance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        if (hasWanderTarget &&
            (playerMovement >= followRefreshDistance ||
             playerDistance >= maximumFollowDistance))
        {
            ChooseFollowingTarget();
        }
    }

    private void ChooseFollowingTarget()
    {
        if (player == null)
        {
            StopMovement();
            return;
        }

        followPosition = (Vector2)player.position + followOffset;
        targetPlayerPosition = player.position;

        Vector2 randomOffset =
            Random.insideUnitCircle * followingWanderRadius;

        wanderTarget = followPosition + randomOffset;

        Vector2 fromPlayer = wanderTarget - (Vector2)player.position;

        if (fromPlayer.magnitude < minimumPlayerDistance)
        {
            Vector2 direction = fromPlayer.sqrMagnitude > 0.001f
                ? fromPlayer.normalized
                : (followOffset.sqrMagnitude > 0.001f
                    ? followOffset.normalized
                    : Vector2.right);

            wanderTarget = (Vector2)player.position +
                           direction * minimumPlayerDistance;
        }

        isWaiting = false;
        hasWanderTarget = true;

        ResetMovementRecovery();
    }

    private void BeginFollowingPause()
    {
        isWaiting = true;
        hasWanderTarget = false;

        wanderWaitTimer = Random.Range(
            Mathf.Max(0f, minWanderWait),
            Mathf.Max(minWanderWait, maxWanderWait)
        );

        ResetMovementRecovery();
        StopMovement();
    }

    // MOVEMENT

    private void MoveTowards(Vector2 target)
    {
        Vector2 toTarget = target - rb.position;

        if (toTarget.magnitude <= arrivalTolerance)
        {
            StopMovement();
            BeginFollowingPause();
            return;
        }

        Vector2 desiredDirection = toTarget.normalized;
        Vector2 direction = GetMovementDirection(desiredDirection);

        if (direction.sqrMagnitude <= 0.001f)
        {
            StopMovement();
            return;
        }

        rb.MovePosition(
            rb.position +
            direction * movementSpeed * Time.fixedDeltaTime
        );
    }

    private Vector2 GetMovementDirection(Vector2 desiredDirection)
    {
        if (desiredDirection.sqrMagnitude <= 0.001f)
            return Vector2.zero;

        if (avoidanceTimer > 0f)
        {
            avoidanceTimer -= Time.fixedDeltaTime;

            if (!IsPathBlocked(avoidanceDirection))
                return avoidanceDirection;

            avoidanceTimer = 0f;
        }

        if (!IsPathBlocked(desiredDirection))
            return desiredDirection;

        Vector2 leftDirection = new Vector2(
            -desiredDirection.y,
            desiredDirection.x
        );

        Vector2 rightDirection = new Vector2(
            desiredDirection.y,
            -desiredDirection.x
        );

        bool leftBlocked = IsPathBlocked(leftDirection);
        bool rightBlocked = IsPathBlocked(rightDirection);

        if (leftBlocked && rightBlocked)
            return Vector2.zero;

        if (leftBlocked)
            avoidanceDirection = rightDirection;
        else if (rightBlocked)
            avoidanceDirection = leftDirection;
        else
            avoidanceDirection = Random.value < 0.5f
                ? leftDirection
                : rightDirection;

        avoidanceTimer = avoidanceDuration;

        return avoidanceDirection;
    }

    private bool IsPathBlocked(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return true;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            direction.normalized,
            obstacleCheckDistance,
            obstacleLayer
        );

        return hit.collider != null;
    }

    // STUCK RECOVERY

    private void CheckStuck()
    {
        stuckCheckTimer += Time.deltaTime;

        if (stuckCheckTimer < stuckCheckInterval)
            return;

        float movedDistance = Vector2.Distance(
            rb.position,
            lastStuckCheckPosition
        );

        if (movedDistance < minimumProgressDistance)
            stuckTimer += stuckCheckTimer;
        else
            stuckTimer = 0f;

        lastStuckCheckPosition = rb.position;
        stuckCheckTimer = 0f;

        if (stuckTimer < maxStuckTime)
            return;

        stuckTimer = 0f;
        avoidanceTimer = 0f;

        ChooseFollowingTarget();
    }

    private void ResetMovementRecovery()
    {
        lastStuckCheckPosition = rb.position;

        stuckCheckTimer = 0f;
        stuckTimer = 0f;
        avoidanceTimer = 0f;

        avoidanceDirection = Vector2.zero;
    }

    private void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // GIZMOS

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        if (hasWanderTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawWireSphere(wanderTarget, arrivalTolerance);
        }

        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(followPosition, followingWanderRadius);
            Gizmos.DrawLine(transform.position, followPosition);
        }
    }
}