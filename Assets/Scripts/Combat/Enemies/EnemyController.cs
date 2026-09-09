using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 100f;

    [Range(0f, 100f)]
    [Tooltip("Percentage of the player's MAX HP dealt per attack.")]
    public float attackDamagePercent = 10f;

    public float movementSpeed = 2f;
    public float defense = 0f;

    [Header("Health Bar")]
    [Tooltip("UI Image set to Filled that represents the enemy's HP.")]
    public Image hpBar;

    [Header("Detection")]
    public float detectionRange = 6f;

    [Header("Combat")]
    public float attackRange = 1.2f;

    [Tooltip("Delay after an attack before the enemy can attack again.")]
    public float attackCooldown = 1f;

    [Tooltip("Delay before the enemy's first attack after reaching attack range.")]
    public float firstAttackDelay = 0.5f;

    [Header("AI Movement")]
    [Tooltip("How close the enemy gets to the player before stopping.")]
    public float stoppingDistance = 1.2f;

    [Tooltip("Layer containing walls/obstacles.")]
    public LayerMask obstacleLayer;

    [Tooltip("How far the enemy checks for obstacles.")]
    public float obstacleCheckDistance = 1f;

    [Header("Knockback")]
    [Tooltip("How much the enemy resists weapon knockback.")]
    public float knockbackResistance = 0f;

    [Tooltip("How long the enemy is affected by knockback.")]
    public float knockbackDuration = 0.15f;

    [Tooltip("How quickly the knockback movement slows down.")]
    public float knockbackDamping = 12f;

    // Components
    private Rigidbody2D rb;

    // Player
    private Transform player;
    private HealthController playerHealth;

    // Health
    private float currentHP;

    // Combat timers
    private float attackTimer;
    private float firstAttackTimer;

    // State
    private bool isAttacking;
    private bool isKnockedBack;

    // Knockback
    private Vector2 knockbackVelocity;
    private float knockbackTimer;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "EnemyController: Rigidbody2D is missing!",
                this
            );
        }
    }


    private void Start()
    {
        // Initialize health
        currentHP = maxHP;
        UpdateHealthBar();

        // Find player
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning(
                "EnemyController: Could not find Player!"
            );

            return;
        }

        player = playerObject.transform;

        playerHealth =
            playerObject.GetComponentInChildren<HealthController>();

        if (playerHealth == null)
        {
            Debug.LogWarning(
                "EnemyController: Could not find HealthController!"
            );
        }

        // Initialize timers
        attackTimer = 0f;
        firstAttackTimer = firstAttackDelay;
    }


    private void Update()
    {
        if (player == null || playerHealth == null)
            return;

        HandleAttackTimer();
        HandleDetectionAndCombat();
    }


    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Knockback takes priority over normal movement.
        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (player == null)
            return;

        MoveTowardsPlayer();
    }


    // AI

    private void HandleAttackTimer()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;
    }


    private void HandleDetectionAndCombat()
    {
        float distance =
            Vector2.Distance(
                rb.position,
                (Vector2)player.position
            );

        // Player is outside detection range.
        if (distance > detectionRange)
        {
            isAttacking = false;
            return;
        }

        // Player is within attack range.
        if (distance <= attackRange)
        {
            isAttacking = true;
            AttackPlayer();
            return;
        }

        // Player is detected but outside attack range.
        isAttacking = false;
    }


    // MOVEMENT

    private void MoveTowardsPlayer()
    {
        float distance =
            Vector2.Distance(
                rb.position,
                player.position
            );

        // Stop when close enough to the player.
        if (distance <= stoppingDistance)
            return;

        Vector2 direction =
            ((Vector2)player.position - rb.position).normalized;

        direction = GetMovementDirection(direction);

        Vector2 movement =
            direction *
            movementSpeed *
            Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);
    }


    private Vector2 GetMovementDirection(Vector2 direction)
    {
        RaycastHit2D obstacle =
            Physics2D.Raycast(
                rb.position,
                direction,
                obstacleCheckDistance,
                obstacleLayer
            );

        // No obstacle in front.
        if (obstacle.collider == null)
            return direction;

        // Try moving left.
        Vector2 leftDirection =
            new Vector2(
                -direction.y,
                direction.x
            ).normalized;

        RaycastHit2D leftCheck =
            Physics2D.Raycast(
                rb.position,
                leftDirection,
                obstacleCheckDistance,
                obstacleLayer
            );

        if (leftCheck.collider == null)
            return leftDirection;

        // Try moving right.
        Vector2 rightDirection =
            -leftDirection;

        RaycastHit2D rightCheck =
            Physics2D.Raycast(
                rb.position,
                rightDirection,
                obstacleCheckDistance,
                obstacleLayer
            );

        if (rightCheck.collider == null)
            return rightDirection;

        // Both sides are blocked.
        return Vector2.zero;
    }


    // ATTACK

    private void AttackPlayer()
    {
        // First attack delay.
        if (firstAttackTimer > 0f)
        {
            firstAttackTimer -= Time.deltaTime;
            return;
        }

        // Attack cooldown.
        if (attackTimer > 0f)
            return;

        float damage =
            playerHealth.maxHealth *
            (attackDamagePercent / 100f);

        playerHealth.TakeDamage(damage);

        Debug.Log(
            gameObject.name +
            " attacked the player for " +
            damage.ToString("F1") +
            " damage."
        );

        attackTimer = attackCooldown;
    }


    // HEALTH

    private void UpdateHealthBar()
    {
        if (hpBar == null)
            return;

        if (maxHP <= 0f)
        {
            hpBar.fillAmount = 0f;
            return;
        }

        hpBar.fillAmount =
            Mathf.Clamp01(
                currentHP / maxHP
            );
    }


    public void TakeDamage(
        float damage,
        Vector2 knockbackDirection,
        float knockbackStrength
    )
    {
        // Apply defense.
        float actualDamage =
            Mathf.Max(
                damage - defense,
                1f
            );

        currentHP -= actualDamage;

        // Prevent negative HP.
        currentHP =
            Mathf.Max(
                currentHP,
                0f
            );

        UpdateHealthBar();

        Debug.Log(
            gameObject.name +
            " took " +
            actualDamage.ToString("F1") +
            " damage. HP: " +
            currentHP.ToString("F1")
        );

        // Apply knockback.
        ApplyKnockback(
            knockbackDirection,
            knockbackStrength
        );

        // Check death.
        if (currentHP <= 0f)
        {
            Die();
        }
    }


    // KNOCKBACK

    private void ApplyKnockback(
        Vector2 direction,
        float strength
    )
    {
        float actualKnockback =
            Mathf.Max(
                strength - knockbackResistance,
                0f
            );

        if (actualKnockback <= 0f)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        knockbackVelocity =
            direction *
            actualKnockback;

        knockbackTimer =
            knockbackDuration;

        isKnockedBack = true;
    }


    private void HandleKnockback()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
        }

        rb.MovePosition(
            rb.position +
            knockbackVelocity *
            Time.fixedDeltaTime
        );

        knockbackVelocity =
            Vector2.MoveTowards(
                knockbackVelocity,
                Vector2.zero,
                knockbackDamping *
                Time.fixedDeltaTime
            );

        // End knockback when either the timer
        // or velocity reaches zero.
        if (
            knockbackTimer <= 0f ||
            knockbackVelocity.sqrMagnitude <= 0.01f
        )
        {
            knockbackVelocity = Vector2.zero;
            knockbackTimer = 0f;
            isKnockedBack = false;
        }
    }


    // DEATH

    private void Die()
    {
        Destroy(gameObject);
    }


    // GIZMOS

    private void OnDrawGizmosSelected()
    {
        // Detection range.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Attack range.
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        // Stopping distance.
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            stoppingDistance
        );

        // Obstacle detection.
        if (player != null)
        {
            Vector2 direction =
                (player.position - transform.position)
                .normalized;

            Gizmos.color = Color.blue;

            Gizmos.DrawLine(
                transform.position,
                transform.position +
                (Vector3)(
                    direction *
                    obstacleCheckDistance
                )
            );
        }
    }
}