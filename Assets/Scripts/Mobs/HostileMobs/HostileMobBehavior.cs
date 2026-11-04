using UnityEngine;
using UnityEngine.UI;

public class HostileMobBehavior : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 100f;

    [Tooltip("Fixed damage dealt by a normal attack.")]
    public float baseDamage = 10f;

    [Range(0f, 100f)]
    [Tooltip("Bonus damage on a critical hit, as a percentage of the player's MAX HP.")]
    public float attackDamagePercent = 10f;

    [Header("Critical Hits")]
    [Range(0f, 100f)]
    [Tooltip("Chance for an attack to deal critical damage.")]
    public float critChance = 20f;

    public float movementSpeed = 2f;
    public float defense = 0f;

    [Header("Mob Drops")]
    public MobDropData mobDropPreset;

    [Header("Health Bar")]
    [Tooltip("UI Image set to Filled that represents the enemy's HP.")]
    public Image hpBar;

    [Header("Detection")]
    public float detectionRange = 6f;

    [Header("Chase")]
    public float chaseRange = 10f;

    [Header("Combat")]
    public float attackRange = 1.5f;

    [Tooltip("Delay before damage is applied after the attack animation starts.")]
    public float attackDamageDelay = 0.3f;

    [Tooltip("Delay after an attack before the enemy can attack again.")]
    public float attackCooldown = 1f;

    [Tooltip("Delay before the enemy's first attack after reaching attack range.")]
    public float firstAttackDelay = 0.5f;

    [Header("AI Movement")]
    [Tooltip("How close the enemy gets to the player before stopping.")]
    public float stoppingDistance = 1.0f;

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

    private Vector2 avoidanceDirection = Vector2.zero;
    private float avoidanceTimer = 0f;

    [Tooltip("How long the enemy commits to an avoidance direction.")]
    public float avoidanceDuration = 0.35f;

    // ATTACK
    private MobAttackAnimator mobAttackAnimator;

    // COMPONENTS
    private Rigidbody2D rb;

    // PLAYER
    private Transform player;
    private HealthController playerHealth;

    // HEALTH
    private float currentHP;
    private HitFlashBehavior hitFlash;

    // COMBAT TIMERS
    private float attackTimer;
    private float firstAttackTimer;

    // STATE
    private bool isAttackAnimationPlaying;
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
            mobAttackAnimator.AttackFinished += OnAttackAnimationFinished;
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
            HealthController health = playerObject.GetComponentInChildren<HealthController>(true);

            if (health != null)
            {
                player = playerObject.transform;
                playerHealth = health;
                break;
            }
        }

        if (player == null || playerHealth == null)
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

        // PREVENT REPEATED ATTACKS
        if (isAttackAnimationPlaying)
            return;

        if (mobAttackAnimator == null)
            return;

        // START ATTACK
        if (mobAttackAnimator.PlayAttack())
        {
            isAttackAnimationPlaying = true;
            StartCoroutine(ApplyDelayedAttackDamage());
        }
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
    }

    // ATTACK ANIMATION FINISHED

    private void OnAttackAnimationFinished()
    {
        isAttackAnimationPlaying = false;
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
        if (mobDropPreset == null || mobDropPreset.drops == null)
            return;

        foreach (MobDropData.DropEntry drop in mobDropPreset.drops)
        {
            if (drop == null || drop.itemPrefab == null)
                continue;

            // ROLL DROP CHANCE
            float chance = Mathf.Clamp(drop.dropChance, 0f, 100f);

            if (chance <= 0f)
                continue;

            if (chance < 100f && Random.Range(0f, 100f) >= chance)
                continue;

            // CALCULATE AMOUNT
            int minAmount = Mathf.Max(1, drop.minAmount);
            int maxAmount = Mathf.Max(minAmount, drop.maxAmount);
            int amount = Random.Range(minAmount, maxAmount + 1);

            // CLONE ITEM
            Item item = drop.itemPrefab.GetComponent<Item>();

            if (item == null)
                continue;

            GameObject droppedItem = item.CloneItem(amount);

            if (droppedItem == null)
                continue;

            droppedItem.transform.position = transform.position;

            // ADD DROP ANIMATION
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
            mobAttackAnimator.AttackFinished -= OnAttackAnimationFinished;
    }
}