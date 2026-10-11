using UnityEngine;
using UnityEngine.UI;

public class PassiveMobController : MonoBehaviour
{
    [Header("Movement")]
    public float movementSpeed = 1.5f;
    public float fleeSpeed = 3f;

    [Header("Attack Response")]
    public float fleeDuration = 3f;

    [Header("Knockback")]
    public float knockbackDuration = 0.15f;
    public float knockbackDamping = 12f;

    private Vector2 knockbackVelocity;
    private float knockbackTimer;
    private bool isKnockedBack;

    [Header("Health")]
    public float maxHP = 100f;
    public Image hpBar;

    [Header("Wandering")]
    public float wanderRadius = 5f;
    public float minWanderWait = 1f;
    public float maxWanderWait = 3f;
    public float destinationThreshold = 0.15f;

    [Header("Obstacle Avoidance")]
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1f;
    public float avoidanceDuration = 0.4f;

    [Header("Stuck Recovery")]
    public float stuckCheckInterval = 1f;
    public float minimumProgressDistance = 0.1f;
    public float maxStuckTime = 2f;

    [Header("Mob Drops")]
    public MobDropData mobDropPreset;

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 startingPosition;
    private Vector2 wanderTarget;
    private Vector2 lastStuckCheckPosition;
    private Vector2 avoidanceDirection;

    private float wanderWaitTimer;
    private float fleeTimer;
    private float currentHP;
    private float avoidanceTimer;
    private float stuckCheckTimer;
    private float stuckTimer;

    private bool isFleeing;
    private bool isWaiting;
    private bool hasWanderTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "PassiveMobController: Rigidbody2D is missing!",
                this
            );
        }
    }

    private void Start()
    {
        if (rb == null)
            return;

        startingPosition = rb.position;
        lastStuckCheckPosition = rb.position;

        currentHP = maxHP;
        UpdateHealthBar();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

        StartWandering();
    }

    private void Update()
    {
        if (rb == null || currentHP <= 0f)
            return;

        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer <= 0f)
                isKnockedBack = false;
        }

        if (isFleeing)
        {
            fleeTimer -= Time.deltaTime;

            if (fleeTimer <= 0f)
            {
                isFleeing = false;
                StartWandering();
            }

            return;
        }

        if (isWaiting)
        {
            wanderWaitTimer -= Time.deltaTime;

            if (wanderWaitTimer <= 0f)
                ChooseWanderTarget();

            return;
        }

        if (!hasWanderTarget)
        {
            ChooseWanderTarget();
            return;
        }

        CheckStuck();
    }

    private void FixedUpdate()
    {
        if (rb == null || currentHP <= 0f)
            return;

        // Knockback temporarily takes priority over AI movement.
        if (isKnockedBack)
        {
            rb.MovePosition(
                rb.position +
                knockbackVelocity * Time.fixedDeltaTime
            );

            knockbackVelocity = Vector2.Lerp(
                knockbackVelocity,
                Vector2.zero,
                knockbackDamping * Time.fixedDeltaTime
            );

            return;
        }

        if (isFleeing && player != null)
        {
            FleeFromPlayer();
            return;
        }

        if (isWaiting || !hasWanderTarget)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveTowards(wanderTarget, movementSpeed);
    }

    // HEALTH

    public void TakeDamage(float damage)
    {
        if (currentHP <= 0f)
            return;

        currentHP = Mathf.Clamp(
            currentHP - damage,
            0f,
            maxHP
        );

        UpdateHealthBar();

        if (currentHP <= 0f)
            Die();
    }

    private void UpdateHealthBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = maxHP > 0f
                ? currentHP / maxHP
                : 0f;
        }
    }

    // ATTACK RESPONSE

    public void ReactToAttack(Transform attacker)
    {
        if (attacker == null || currentHP <= 0f)
            return;

        player = attacker;

        isFleeing = true;
        fleeTimer = fleeDuration;

        isWaiting = false;
        hasWanderTarget = false;

        avoidanceTimer = 0f;
        stuckTimer = 0f;
    }

    public void ApplyKnockback(Vector2 direction, float strength)
    {
        if (currentHP <= 0f)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        knockbackVelocity = direction.normalized * strength;
        knockbackTimer = knockbackDuration;
        isKnockedBack = true;
    }

    // DEATH AND DROPS

    private void Die()
    {
        DropItems();
        Destroy(gameObject);
    }

    private void DropItems()
    {
        if (mobDropPreset == null || mobDropPreset.drops == null)
            return;

        foreach (MobDropData.DropEntry drop in mobDropPreset.drops)
        {
            if (drop == null || drop.itemPrefab == null)
                continue;

            float dropChance = Mathf.Clamp(
                drop.dropChance,
                0f,
                100f
            );

            if (dropChance <= 0f)
                continue;

            if (dropChance < 100f &&
                Random.Range(0f, 100f) >= dropChance)
            {
                continue;
            }

            int minAmount = Mathf.Max(1, drop.minAmount);
            int maxAmount = Mathf.Max(minAmount, drop.maxAmount);

            int amount = Random.Range(minAmount, maxAmount + 1);

            Item item = drop.itemPrefab.GetComponent<Item>();

            if (item == null)
                continue;

            GameObject droppedItem = item.CloneItem(amount);

            if (droppedItem != null)
                droppedItem.transform.position = transform.position;
        }
    }

    // WANDERING

    private void StartWandering()
    {
        isWaiting = true;
        hasWanderTarget = false;

        stuckTimer = 0f;
        stuckCheckTimer = 0f;
        avoidanceTimer = 0f;

        wanderWaitTimer = Random.Range(
            Mathf.Max(0f, minWanderWait),
            Mathf.Max(minWanderWait, maxWanderWait)
        );

        rb.linearVelocity = Vector2.zero;
    }

    private void ChooseWanderTarget()
    {
        // Choose a target once, then keep it until arrival or recovery.
        Vector2 randomOffset =
            Random.insideUnitCircle * wanderRadius;

        wanderTarget = startingPosition + randomOffset;

        isWaiting = false;
        hasWanderTarget = true;

        stuckTimer = 0f;
        stuckCheckTimer = 0f;
        lastStuckCheckPosition = rb.position;
        avoidanceTimer = 0f;
    }

    private void CheckWanderArrival()
    {
        if (!hasWanderTarget)
            return;

        if (Vector2.Distance(rb.position, wanderTarget)
            <= destinationThreshold)
        {
            StartWandering();
        }
    }

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

        if (stuckTimer >= maxStuckTime)
        {
            // Abandon the unreachable target and try another.
            ChooseWanderTarget();
        }
    }

    // FLEEING

    private void FleeFromPlayer()
    {
        Vector2 direction =
            rb.position - (Vector2)player.position;

        if (direction.sqrMagnitude <= 0.001f)
            direction = Vector2.right;

        Vector2 target = rb.position + direction.normalized * 2f;

        MoveTowards(target, fleeSpeed);
    }

    // MOVEMENT AND OBSTACLE AVOIDANCE

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 toTarget = target - rb.position;

        if (toTarget.sqrMagnitude <=
            destinationThreshold * destinationThreshold)
        {
            rb.linearVelocity = Vector2.zero;

            if (!isFleeing)
                CheckWanderArrival();

            return;
        }

        Vector2 desiredDirection = toTarget.normalized;
        Vector2 direction = GetMovementDirection(desiredDirection);

        if (direction.sqrMagnitude <= 0.001f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(
            rb.position + direction * speed * Time.fixedDeltaTime
        );

        if (!isFleeing)
            CheckWanderArrival();
    }

    private Vector2 GetMovementDirection(Vector2 desiredDirection)
    {
        // Keep the chosen avoidance direction briefly to prevent
        // left-right switching on consecutive physics frames.
        if (avoidanceTimer > 0f)
        {
            avoidanceTimer -= Time.fixedDeltaTime;

            if (!IsPathBlocked(avoidanceDirection))
                return desiredDirection;

            return avoidanceDirection;
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

        if (!leftBlocked && !rightBlocked)
        {
            // Prefer the side that points more toward the target.
            float leftScore = Vector2.Dot(
                leftDirection,
                desiredDirection
            );

            float rightScore = Vector2.Dot(
                rightDirection,
                desiredDirection
            );

            avoidanceDirection = leftScore >= rightScore
                ? leftDirection
                : rightDirection;
        }
        else if (!leftBlocked)
        {
            avoidanceDirection = leftDirection;
        }
        else if (!rightBlocked)
        {
            avoidanceDirection = rightDirection;
        }
        else
        {
            // Both sides are blocked. Stop rather than pushing
            // into a wall; stuck recovery will select a new target.
            return Vector2.zero;
        }

        avoidanceTimer = Mathf.Max(0f, avoidanceDuration);

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

    // GIZMOS

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            Application.isPlaying
                ? (Vector3)startingPosition
                : transform.position,
            wanderRadius
        );

        if (Application.isPlaying && hasWanderTarget)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, wanderTarget);
            Gizmos.DrawWireSphere(wanderTarget, destinationThreshold);
        }
    }
}