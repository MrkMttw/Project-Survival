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

    private float currentHP;

    private Transform player;
    private HealthController playerHealth;

    private float attackTimer;
    private float firstAttackTimer;

    private bool isAttacking;

    // Knockback
    private Vector2 knockbackVelocity;
    private bool isKnockedBack;

    private void Start()
    {
        // Initialize HP
        currentHP = maxHP;

        // Initialize HP bar
        UpdateHealthBar();

        // Find player
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerHealth =
                playerObject.GetComponentInChildren<HealthController>();

            if (playerHealth == null)
            {
                Debug.LogWarning(
                    "EnemyController: Could not find HealthController!"
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "EnemyController: Could not find Player!"
            );
        }

        attackTimer = 0f;
        firstAttackTimer = firstAttackDelay;
    }

    private void Update()
    {
        // Knockback takes priority over normal AI.
        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (player == null)
            return;

        if (playerHealth == null)
            return;

        // Reduce attack cooldown
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // DETECTION
        if (distance > detectionRange)
        {
            isAttacking = false;
            return;
        }

        // ATTACK RANGE
        if (distance <= attackRange)
        {
            isAttacking = true;

            AttackPlayer();
            return;
        }

        // CHASE
        isAttacking = false;

        ChasePlayer();
    }

    // HEALTH BAR

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
            Mathf.Clamp01(currentHP / maxHP);
    }

    // CHASE

    private void ChasePlayer()
    {
        Vector2 direction =
            (player.position - transform.position).normalized;

        // Check for obstacle
        RaycastHit2D obstacle = Physics2D.Raycast(
            transform.position,
            direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (obstacle.collider != null)
        {
            // Try to move around the obstacle
            Vector2 perpendicular =
                new Vector2(-direction.y, direction.x);

            RaycastHit2D leftCheck = Physics2D.Raycast(
                transform.position,
                perpendicular,
                obstacleCheckDistance,
                obstacleLayer
            );

            RaycastHit2D rightCheck = Physics2D.Raycast(
                transform.position,
                -perpendicular,
                obstacleCheckDistance,
                obstacleLayer
            );

            if (leftCheck.collider == null)
            {
                direction = perpendicular;
            }
            else if (rightCheck.collider == null)
            {
                direction = -perpendicular;
            }
        }

        transform.position +=
            (Vector3)(
                direction *
                movementSpeed *
                Time.deltaTime
            );
    }

    // ATTACK PLAYER

    private void AttackPlayer()
    {
        if (firstAttackTimer > 0f)
        {
            firstAttackTimer -= Time.deltaTime;
            return;
        }

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

    // PLAYER WEAPON DAMAGE

    public void TakeDamage(
        float damage,
        Vector2 knockbackDirection,
        float knockbackStrength
    )
    {
        // Apply enemy defense
        float actualDamage =
            Mathf.Max(damage - defense, 1f);

        currentHP -= actualDamage;

        // Prevent HP from going below zero
        currentHP =
            Mathf.Max(currentHP, 0f);

        // Update HP bar
        UpdateHealthBar();

        Debug.Log(
            gameObject.name +
            " took " +
            actualDamage.ToString("F1") +
            " damage. HP: " +
            currentHP.ToString("F1")
        );

        // Apply controlled knockback
        ApplyKnockback(
            knockbackDirection,
            knockbackStrength
        );

        // Check death
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
            direction * actualKnockback;

        isKnockedBack = true;
    }

    private void HandleKnockback()
    {
        transform.position +=
            (Vector3)(
                knockbackVelocity *
                Time.deltaTime
            );

        knockbackVelocity =
            Vector2.MoveTowards(
                knockbackVelocity,
                Vector2.zero,
                knockbackDamping *
                Time.deltaTime
            );

        if (knockbackVelocity.sqrMagnitude <= 0.01f)
        {
            knockbackVelocity = Vector2.zero;
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
        // Detection range
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Attack range
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        // Obstacle detection
        if (player != null)
        {
            Vector2 direction =
                (player.position - transform.position).normalized;

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