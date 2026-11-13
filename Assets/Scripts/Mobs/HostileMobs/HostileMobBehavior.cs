using UnityEngine;
using UnityEngine.UI;

public class HostileMobBehavior : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 100f;
    public float baseDamage = 10f;

    [Range(0f, 100f)]
    public float attackDamagePercent = 10f;

    [Header("Critical Hits")]
    [Range(0f, 100f)]
    public float critChance = 20f;

    public float movementSpeed = 2f;
    public float defense = 0f;

    [Header("Mob Drops")]
    public MobDropData mobDropPreset;

    [Header("Health Bar")]
    public Image hpBar;

    [Header("Detection")]
    public float detectionRange = 6f;

    [Header("Chase")]
    public float chaseRange = 10f;

    [Header("Combat")]
    public float attackRange = 1.5f;

    [Header("Player Knockback")]
    public float playerKnockbackStrength = 5f;
    public float playerKnockbackDuration = 0.2f;

    [Tooltip("Delay before damage is applied after the attack animation starts.")]
    public float attackDamageDelay = 0.3f;

    public float attackCooldown = 1f;
    public float firstAttackDelay = 0.5f;

    [Header("AI Movement")]
    public float stoppingDistance = 1.0f;
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1f;

    [Header("Knockback")]
    public float knockbackResistance = 0f;
    public float knockbackDuration = 0.15f;
    public float knockbackDamping = 12f;

    [Tooltip("How long the enemy commits to an avoidance direction.")]
    public float avoidanceDuration = 0.35f;

    private Vector2 avoidanceDirection = Vector2.zero;
    private float avoidanceTimer = 0f;

    // ATTACK
    private MobAttackAnimator mobAttackAnimator;
    private bool isAttackAnimationLocked;

    public bool IsAttackAnimationLocked => isAttackAnimationLocked;

    // COMPONENTS
    private Rigidbody2D rb;

    // PLAYER
    private Transform player;
    private PlayerMovement playerMovement;
    private HealthController playerHealth;

    // HEALTH
    private float currentHP;
    private HitFlashBehavior hitFlash;

    // COMBAT TIMERS
    private float attackTimer;
    private float firstAttackTimer;

    // STATE
    private bool isChasing;
    private bool isAttacking;
    private bool isKnockedBack;
    private bool isDead;

    // KNOCKBACK
    private Vector2 knockbackVelocity;
    private float knockbackTimer;

    private void Awake()
    {
        hitFlash = GetComponent<HitFlashBehavior>();
        rb = GetComponent<Rigidbody2D>();

        mobAttackAnimator = GetComponentInChildren<MobAttackAnimator>(true);

        if (mobAttackAnimator != null)
        {
            mobAttackAnimator.AttackStarted += OnAttackStarted;
            mobAttackAnimator.AttackFinished += OnAttackFinished;
        }
    }

    private void Start()
    {
        currentHP = maxHP;
        UpdateHealthBar();

        // FIND PLAYER
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObject in players)
        {
            HealthController health =
                playerObject.GetComponentInChildren<HealthController>(true);

            PlayerMovement movement =
                playerObject.GetComponentInChildren<PlayerMovement>(true);

            if (health != null && movement != null)
            {
                player = playerObject.transform;
                playerHealth = health;
                playerMovement = movement;

                break;
            }
        }

        if (player == null || playerHealth == null || playerMovement == null)
            return;

        // INITIALIZE TIMERS
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

        // KNOCKBACK
        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (player == null)
            return;

        // ONLY MOVE WHILE CHASING
        if (!isChasing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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
        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        // DETECT PLAYER
        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
            firstAttackTimer = firstAttackDelay;
        }

        // PLAYER ESCAPED CHASE RANGE
        if (isChasing && distance > chaseRange)
        {
            isChasing = false;
            isAttacking = false;

            // CANCEL INTERRUPTED ATTACK
            mobAttackAnimator?.CancelAttack();

            return;
        }

        // NOT CHASING
        if (!isChasing)
        {
            isAttacking = false;
            return;
        }

        // ATTACK PLAYER
        if (distance <= attackRange)
        {
            isAttacking = true;
            AttackPlayer();
            return;
        }

        isAttacking = false;
    }

    // MOVEMENT

    private void MoveTowardsPlayer()
    {
        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        if (distance <= stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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
        if (avoidanceTimer > 0f)
        {
            avoidanceTimer -= Time.fixedDeltaTime;
            return avoidanceDirection;
        }

        RaycastHit2D obstacle = Physics2D.Raycast(
            rb.position,
            direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (obstacle.collider == null)
        {
            avoidanceDirection = direction;
            return direction;
        }

        Vector2 leftDirection = new Vector2(
            -direction.y,
            direction.x
        ).normalized;

        Vector2 rightDirection = new Vector2(
            direction.y,
            -direction.x
        ).normalized;

        RaycastHit2D leftCheck = Physics2D.Raycast(
            rb.position,
            leftDirection,
            obstacleCheckDistance,
            obstacleLayer
        );

        RaycastHit2D rightCheck = Physics2D.Raycast(
            rb.position,
            rightDirection,
            obstacleCheckDistance,
            obstacleLayer
        );

        if (leftCheck.collider == null && rightCheck.collider == null)
        {
            if (Vector2.Dot(leftDirection, direction) >=
                Vector2.Dot(rightDirection, direction))
            {
                avoidanceDirection = leftDirection;
            }
            else
            {
                avoidanceDirection = rightDirection;
            }
        }
        else if (leftCheck.collider == null)
        {
            avoidanceDirection = leftDirection;
        }
        else if (rightCheck.collider == null)
        {
            avoidanceDirection = rightDirection;
        }
        else
        {
            avoidanceDirection = direction;
        }

        avoidanceTimer = avoidanceDuration;

        return avoidanceDirection;
    }

    // ATTACK
    private void AttackPlayer()
    {
        if (isKnockedBack || isDead)
            return;

        // FIRST ATTACK DELAY
        if (firstAttackTimer > 0f)
        {
            firstAttackTimer -= Time.deltaTime;
            return;
        }

        // ATTACK COOLDOWN
        if (attackTimer > 0f)
            return;

        // CHECK IF ANIMATION CAN PLAY
        if (mobAttackAnimator != null)
        {
            if (!mobAttackAnimator.PlayAttack())
                return;
        }

        // START ATTACK COOLDOWN
        attackTimer = attackCooldown;

        // APPLY DELAYED DAMAGE
        StartCoroutine(ApplyDelayedAttackDamage());
    }

    private void OnAttackStarted()
    {
        isAttackAnimationLocked = true;
    }

    private void OnAttackFinished()
    {
        isAttackAnimationLocked = false;
    }

    // APPLY DELAYED ATTACK DAMAGE

    private System.Collections.IEnumerator ApplyDelayedAttackDamage()
    {
        yield return new WaitForSeconds(attackDamageDelay);

        if (isDead || isKnockedBack)
            yield break;

        ApplyAttackDamage();
    }

    private void ApplyAttackDamage()
    {
        if (isDead || isKnockedBack || player == null || playerHealth == null)
            return;

        // CHECK IF PLAYER IS STILL IN ATTACK RANGE
        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        if (distance > attackRange)
            return;

        // CALCULATE DAMAGE
        float damage = baseDamage;

        bool isCritical = Random.Range(0f, 100f) < critChance;

        if (isCritical)
        {
            damage += playerHealth.maxHealth *
                      (attackDamagePercent / 100f);
        }

        // APPLY DAMAGE
        playerHealth.TakeDamage(damage);

        // APPLY PLAYER KNOCKBACK
        if (playerMovement != null)
        {
            Vector2 knockbackDirection =
                ((Vector2)player.position - rb.position).normalized;

            playerMovement.ApplyKnockback(
                knockbackDirection,
                playerKnockbackStrength,
                playerKnockbackDuration
            );
        }
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

        hpBar.fillAmount = Mathf.Clamp01(currentHP / maxHP);
    }

    public void TakeDamage(
        float damage,
        Vector2 knockbackDirection,
        float knockbackStrength
    )
    {
        if (isDead)
            return;

        float actualDamage = Mathf.Max(damage - defense, 1f);

        currentHP -= actualDamage;
        currentHP = Mathf.Max(currentHP, 0f);

        UpdateHealthBar();

        // PLAY HIT FLASH
        hitFlash?.PlayHitFlash();

        // APPLY KNOCKBACK
        ApplyKnockback(
            knockbackDirection,
            knockbackStrength
        );

        // CHECK DEATH
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
        float actualKnockback = Mathf.Max(
            strength - knockbackResistance,
            0f
        );

        if (actualKnockback <= 0f)
            return;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        knockbackVelocity = direction * actualKnockback;
        knockbackTimer = knockbackDuration;
        isKnockedBack = true;

        // CANCEL CURRENT ATTACK
        mobAttackAnimator?.CancelAttack();
    }

    private void HandleKnockback()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
        }

        rb.MovePosition(
            rb.position +
            knockbackVelocity * Time.fixedDeltaTime
        );

        knockbackVelocity = Vector2.MoveTowards(
            knockbackVelocity,
            Vector2.zero,
            knockbackDamping * Time.fixedDeltaTime
        );

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
        if (isDead)
            return;

        isDead = true;

        // PLAY DETACHED SMOKE
        SmokeEffectAnimator smokeEffect =
            GetComponentInChildren<SmokeEffectAnimator>();

        GameObject detachedSmoke = null;

        if (smokeEffect != null)
        {
            detachedSmoke = smokeEffect.PlaySmokeDetached();
        }

        // BOSS DEFEAT REGISTRATION
        StructureObjectIdentity identity =
            GetComponent<StructureObjectIdentity>();

        if (identity != null && identity.isBoss)
        {
            StructureSpawner structureSpawner =
                FindFirstObjectByType<StructureSpawner>();

            if (structureSpawner != null)
                structureSpawner.MarkBossDefeated(identity);
        }

        // DROP ITEMS
        DropItems();

        // DESTROY MOB
        Destroy(gameObject);

        // CLEANUP SMOKE
        if (detachedSmoke != null)
        {
            Destroy(detachedSmoke, smokeEffect.SmokeDuration);
        }
    }

    private void DropItems()
    {
        if (
            mobDropPreset == null ||
            mobDropPreset.drops == null
        )
            return;

        if (DropLootController.Instance == null)
        {
            Debug.LogError(
                "HostileMobBehavior: DropLootController instance is missing!"
            );
            return;
        }

        foreach (MobDropData.DropEntry drop in mobDropPreset.drops)
        {
            if (drop == null || drop.itemPrefab == null)
                continue;

            // DROP CHANCE
            float chance = Mathf.Clamp(
                drop.dropChance,
                0f,
                100f
            );

            if (chance <= 0f)
                continue;

            if (
                chance < 100f &&
                Random.Range(0f, 100f) >= chance
            )
                continue;

            // DROP AMOUNT
            int minAmount = Mathf.Max(1, drop.minAmount);
            int maxAmount = Mathf.Max(minAmount, drop.maxAmount);

            int amount = Random.Range(
                minAmount,
                maxAmount + 1
            );

            // SPAWN THROUGH DROP LOOT CONTROLLER
            DropLootController.Instance.SpawnLoot(
                drop.itemPrefab,
                amount,
                transform.position
            );
        }
    }

    // GIZMOS

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

    private void OnDestroy()
    {
        if (mobAttackAnimator != null)
        {
            mobAttackAnimator.AttackStarted -= OnAttackStarted;
            mobAttackAnimator.AttackFinished -= OnAttackFinished;
        }
    }
}