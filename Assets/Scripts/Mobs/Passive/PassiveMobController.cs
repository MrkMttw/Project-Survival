
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

    [Header("Mob Drops")]
    public MobDropData mobDropPreset;

    private Rigidbody2D rb;
    private Transform player;

    private Vector2 startingPosition;
    private Vector2 wanderTarget;

    private float wanderWaitTimer;
    private float fleeTimer;
    private float currentHP;

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

        currentHP = maxHP;
        UpdateHealthBar();

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        StartWandering();
    }

    private void Update()
    {
        if (rb == null)
            return;

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
            ChooseWanderTarget();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

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
        {
            Die();
        }
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
        if (attacker == null)
            return;

        player = attacker;
        isFleeing = true;
        fleeTimer = fleeDuration;

        isWaiting = false;
        hasWanderTarget = false;
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

    // DEATH
    
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

            int amount = Random.Range(
                minAmount,
                maxAmount + 1
            );

            Item item = drop.itemPrefab.GetComponent<Item>();

            if (item == null)
                continue;

            GameObject droppedItem = item.CloneItem(amount);

            if (droppedItem != null)
            {
                droppedItem.transform.position = transform.position;
            }
        }
    }

    // WANDERING

    private void StartWandering()
    {
        isWaiting = true;
        hasWanderTarget = false;

        wanderWaitTimer = Random.Range(
            minWanderWait,
            maxWanderWait
        );
    }

    private void ChooseWanderTarget()
    {
        Vector2 randomOffset =
            Random.insideUnitCircle * wanderRadius;

        wanderTarget = startingPosition + randomOffset;

        isWaiting = false;
        hasWanderTarget = true;
    }

    private void CheckWanderArrival()
    {
        if (!hasWanderTarget)
            return;

        if (Vector2.Distance(
            rb.position,
            wanderTarget
        ) <= destinationThreshold)
        {
            StartWandering();
        }
    }

    // FLEEING
    
    private void FleeFromPlayer()
    {
        Vector2 direction =
            rb.position - (Vector2)player.position;

        if (direction.sqrMagnitude <= 0.001f)
            direction = Random.insideUnitCircle.normalized;

        MoveTowards(
            rb.position + direction.normalized * 2f,
            fleeSpeed
        );
    }

    // MOVEMENT AND OBSTACLES

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = target - rb.position;

        if (direction.sqrMagnitude <=
            destinationThreshold * destinationThreshold)
        {
            rb.linearVelocity = Vector2.zero;

            if (!isFleeing)
                CheckWanderArrival();

            return;
        }

        direction.Normalize();
        direction = GetMovementDirection(direction);

        Vector2 movement =
            direction * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        if (!isFleeing)
            CheckWanderArrival();
    }

    private Vector2 GetMovementDirection(Vector2 direction)
    {
        RaycastHit2D obstacle = Physics2D.Raycast(
            rb.position,
            direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (obstacle.collider == null)
            return direction;

        Vector2 leftDirection = new Vector2(
            -direction.y,
            direction.x
        ).normalized;

        RaycastHit2D leftCheck = Physics2D.Raycast(
            rb.position,
            leftDirection,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (leftCheck.collider == null)
            return leftDirection;

        Vector2 rightDirection = new Vector2(
            direction.y,
            -direction.x
        ).normalized;

        RaycastHit2D rightCheck = Physics2D.Raycast(
            rb.position,
            rightDirection,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (rightCheck.collider == null)
            return rightDirection;

        return Vector2.zero;
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
    }
}