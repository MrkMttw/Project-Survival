using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileMobBehavior : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 100f;
    public float defense = 0f;

    [Header("Mob Drops")]
    public MobDropData mobDropPreset;

    [Header("Health Bar")]
    public Image hpBar;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float chaseRange = 12f;

    [Header("Chase Movement")]
    public float movementSpeed = 2f;
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1f;
    public float avoidanceDuration = 0.35f;

    [Header("Maintain Distance")]
    public float preferredDistance = 5f;

    [Header("Ranged Combat")]
    public float attackRange = 8f;
    public float attackCooldown = 1.5f;
    public float firstAttackDelay = 0.5f;
    public float projectileSpawnDelay = 0.3f;
    public float attackAnimationDuration = 0.8f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform attackPoint;
    public float projectileSpeed = 10f;
    public float projectileMaxDistance = 10f;
    public float projectileMaxLifetime = 3f;
    public float projectileDamage = 10f;
    public float projectileKnockbackStrength = 5f;
    public float projectileKnockbackDuration = 0.2f;

    [Tooltip("Rotation adjustment for the projectile sprite.")]
    public float projectileRotationOffset = -45f;

    [Header("Mob Knockback")]
    public float knockbackResistance = 0f;
    public float knockbackDuration = 0.15f;
    public float knockbackDamping = 12f;

    [Header("Animation")]
    public MobAttackAnimator mobAttackAnimator;

    [Header("Smoke Effect")]
    public SmokeEffectAnimator smokeEffectAnimator;

    // COMPONENTS
    private Rigidbody2D rb;
    private HitFlashBehavior hitFlash;

    // PLAYER
    private Transform player;
    private PlayerMovement playerMovement;
    private HealthController playerHealth;

    // HEALTH
    private float currentHP;
    private bool isDead;

    // MOVEMENT
    private bool isPlayerDetected;
    private Vector2 avoidanceDirection;
    private float avoidanceTimer;

    // COMBAT
    private float attackTimer;
    private float firstAttackTimer;
    private float projectileSpawnTimer;
    private float attackAnimationTimer;

    private bool isAttackAnimationPlaying;
    private bool hasFiredProjectile;

    // KNOCKBACK
    private bool isKnockedBack;
    private Vector2 knockbackVelocity;
    private float knockbackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitFlash = GetComponent<HitFlashBehavior>();

        // FIND ATTACK ANIMATOR
        if (mobAttackAnimator == null)
        {
            mobAttackAnimator =
                GetComponentInChildren<MobAttackAnimator>(true);
        }

        // FIND SMOKE EFFECT
        if (smokeEffectAnimator == null)
        {
            smokeEffectAnimator =
                GetComponentInChildren<SmokeEffectAnimator>(true);
        }
    }

    private void Start()
    {
        currentHP = maxHP;
        UpdateHealthBar();

        FindPlayer();

        firstAttackTimer = firstAttackDelay;
    }

    private void Update()
    {
        if (isDead || player == null || playerHealth == null)
            return;

        HandleAttackTimer();
        HandleAttackSequence();
        HandleDetection();
        HandleCombat();
    }

    private void FixedUpdate()
    {
        if (rb == null || isDead)
            return;

        // KNOCKBACK
        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (player == null || !isPlayerDetected)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // STOP DURING ATTACK
        if (isAttackAnimationPlaying)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveTowardsPlayer();
    }

    // PLAYER DETECTION

    private void FindPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

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
    }

    private void HandleDetection()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        // DETECT PLAYER
        if (!isPlayerDetected && distance <= detectionRange)
        {
            isPlayerDetected = true;
            firstAttackTimer = firstAttackDelay;
        }

        // PLAYER ESCAPED CHASE RANGE
        if (isPlayerDetected && distance > chaseRange)
        {
            isPlayerDetected = false;
        }
    }

    // COMBAT TIMERS

    private void HandleAttackTimer()
    {
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (firstAttackTimer > 0f && isPlayerDetected)
            firstAttackTimer -= Time.deltaTime;
    }

    private void HandleAttackSequence()
    {
        if (!isAttackAnimationPlaying)
            return;

        // PROJECTILE SPAWN DELAY
        if (!hasFiredProjectile)
        {
            projectileSpawnTimer -= Time.deltaTime;

            if (projectileSpawnTimer <= 0f)
            {
                FireProjectile();
                hasFiredProjectile = true;
            }
        }

        // ATTACK ANIMATION DURATION
        attackAnimationTimer -= Time.deltaTime;

        // FINISH ATTACK SEQUENCE
        if (
            attackAnimationTimer <= 0f &&
            hasFiredProjectile
        )
        {
            isAttackAnimationPlaying = false;
            attackTimer = attackCooldown;
        }
    }

    private void HandleCombat()
    {
        if (!isPlayerDetected || isKnockedBack || isDead)
            return;

        if (isAttackAnimationPlaying)
            return;

        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        // CHECK ATTACK RANGE
        if (distance > attackRange)
            return;

        // WAIT UNTIL PREFERRED DISTANCE IS REACHED
        if (distance > preferredDistance)
            return;

        // FIRST ATTACK DELAY
        if (firstAttackTimer > 0f)
            return;

        // ATTACK COOLDOWN
        if (attackTimer > 0f)
            return;

        if (mobAttackAnimator == null)
            return;

        // START ATTACK ANIMATION
        if (mobAttackAnimator.PlayAttack())
        {
            isAttackAnimationPlaying = true;
            hasFiredProjectile = false;

            projectileSpawnTimer = projectileSpawnDelay;
            attackAnimationTimer = attackAnimationDuration;
        }
    }

    // PROJECTILE ATTACK

    private void FireProjectile()
    {
        if (
            isDead ||
            isKnockedBack ||
            !isPlayerDetected ||
            player == null
        )
            return;

        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        // CHECK IF PLAYER IS STILL IN RANGE
        if (
            distance > attackRange ||
            distance > preferredDistance
        )
            return;

        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "ProjectileMobBehavior: Projectile prefab is missing!"
            );
            return;
        }

        if (attackPoint == null)
        {
            Debug.LogWarning(
                "ProjectileMobBehavior: Attack point is missing!"
            );
            return;
        }

        // CALCULATE SHOOTING DIRECTION
        Vector2 direction =
            ((Vector2)player.position -
             (Vector2)attackPoint.position).normalized;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        // CALCULATE PROJECTILE ROTATION
        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle + projectileRotationOffset
            );

        // SPAWN PROJECTILE
        GameObject projectile = Instantiate(
            projectilePrefab,
            attackPoint.position,
            rotation
        );

        MobProjectile projectileBehavior =
            projectile.GetComponent<MobProjectile>();

        if (projectileBehavior == null)
        {
            Debug.LogError(
                "ProjectileMobBehavior: Projectile prefab has no MobProjectile component!"
            );

            Destroy(projectile);
            return;
        }

        // INITIALIZE PROJECTILE
        projectileBehavior.Initialize(
            direction,
            transform,
            projectileSpeed,
            projectileMaxDistance,
            projectileMaxLifetime,
            projectileDamage,
            projectileKnockbackStrength,
            projectileKnockbackDuration
        );
    }

    // MOVEMENT

    private void MoveTowardsPlayer()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(
            rb.position,
            (Vector2)player.position
        );

        // MAINTAIN DISTANCE
        // NEVER MOVE AWAY FROM PLAYER
        if (distance <= preferredDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction =
            ((Vector2)player.position - rb.position).normalized;

        Vector2 movementDirection =
            GetMovementDirection(direction);

        rb.MovePosition(
            rb.position +
            movementDirection *
            movementSpeed *
            Time.fixedDeltaTime
        );
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

        if (
            leftCheck.collider == null &&
            rightCheck.collider == null
        )
        {
            if (
                Vector2.Dot(leftDirection, direction) >=
                Vector2.Dot(rightDirection, direction)
            )
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

    // HEALTH

    public void TakeDamage(
        float damage,
        Vector2 knockbackDirection,
        float knockbackStrength
    )
    {
        if (isDead)
            return;

        float actualDamage = Mathf.Max(
            damage - defense,
            1f
        );

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

    private void UpdateHealthBar()
    {
        if (hpBar == null)
            return;

        if (maxHP <= 0f)
        {
            hpBar.fillAmount = 0f;
            return;
        }

        hpBar.fillAmount = Mathf.Clamp01(
            currentHP / maxHP
        );
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

        if (
            actualKnockback <= 0f ||
            direction.sqrMagnitude <= 0.001f
        )
            return;

        direction.Normalize();

        knockbackVelocity = direction * actualKnockback;
        knockbackTimer = knockbackDuration;
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

        rb.linearVelocity = Vector2.zero;

        // PLAY DETACHED SMOKE
        GameObject detachedSmoke = null;

        if (smokeEffectAnimator != null)
        {
            detachedSmoke =
                smokeEffectAnimator.PlaySmokeDetached();
        }

        // BOSS DEFEAT REGISTRATION
        StructureObjectIdentity identity =
            GetComponent<StructureObjectIdentity>();

        if (identity != null && identity.isBoss)
        {
            StructureSpawner structureSpawner =
                FindFirstObjectByType<StructureSpawner>();

            if (structureSpawner != null)
            {
                structureSpawner.MarkBossDefeated(identity);
            }
        }

        // DROP ITEMS
        DropItems();

        // DESTROY MOB
        Destroy(gameObject);

        // CLEANUP SMOKE
        if (detachedSmoke != null)
        {
            Destroy(
                detachedSmoke,
                smokeEffectAnimator.SmokeDuration
            );
        }
    }

    private void DropItems()
    {
        if (
            mobDropPreset == null ||
            mobDropPreset.drops == null
        )
            return;

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

            // CLONE ITEM
            Item item = drop.itemPrefab.GetComponent<Item>();

            if (item == null)
                continue;

            GameObject droppedItem = item.CloneItem(amount);

            if (droppedItem == null)
                continue;

            droppedItem.transform.position = transform.position;

            // LOOT DROP ANIMATION
            LootDropAnimation dropAnimation =
                droppedItem.GetComponent<LootDropAnimation>();

            if (dropAnimation == null)
            {
                dropAnimation =
                    droppedItem.AddComponent<LootDropAnimation>();
            }

            dropAnimation.Play();
        }
    }

    // GIZMOS

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            chaseRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position,
            preferredDistance
        );
    }
}