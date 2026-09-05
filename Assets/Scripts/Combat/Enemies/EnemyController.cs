using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 100f;

    [Range(0f, 100f)]
    [Tooltip("Percentage of the player's MAX HP dealt per attack.")]
    public float attackDamagePercent = 10f;

    public float movementSpeed = 2f;
    public float defense = 0f;

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

    private float currentHP;

    private Transform player;
    private HealthController playerHealth;

    private float attackTimer;
    private float firstAttackTimer;

    private bool isAttacking;

    private void Start()
    {
        currentHP = maxHP;

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
        if (player == null)
            return;

        if (playerHealth == null)
            return;

        // Reduce timers
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // DETECT

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

    private void AttackPlayer()
    {
        if (firstAttackTimer > 0f)
        {
            firstAttackTimer -= Time.deltaTime;
            return;
        }

        if (attackTimer > 0f)
            return;

        float damage = playerHealth.maxHealth *
                    (attackDamagePercent / 100f);

        playerHealth.TakeDamage(damage);

        Debug.Log(
            gameObject.name +
            " attacked the player for " +
            damage +
            " damage (" +
            attackDamagePercent +
            "% of max HP)"
        );

        attackTimer = attackCooldown;
    }

    public void TakeDamage(float damage)
    {
        float actualDamage =
            Mathf.Max(damage - defense, 1f);

        currentHP -= actualDamage;

        Debug.Log(
            gameObject.name +
            " took " +
            actualDamage +
            " damage. HP: " +
            currentHP
        );

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

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
                (Vector3)(direction * obstacleCheckDistance)
            );
        }
    }
}