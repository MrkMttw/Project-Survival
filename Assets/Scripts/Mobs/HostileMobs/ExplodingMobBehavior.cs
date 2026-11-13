using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class ExplodingMobBehavior : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHP = 100f;
    public float defense = 0f;

    [Header("Mob Drops")]
    public MobDropData mobDropPreset;

    [Header("Health Bar")]
    public Image hpBar;

    [Header("Fuse Countdown Text")]
    public TMP_Text fuseTimerText;

    [Header("Detection")]
    public float detectionRange = 6f;
    public float chaseRange = 10f;

    [Header("Chase Movement")]
    public float movementSpeed = 2f;
    public float stoppingDistance = 1f;
    public LayerMask obstacleLayer;
    public float obstacleCheckDistance = 1f;
    public float avoidanceDuration = 0.35f;

    [Header("Maintain Distance")]
    public float maintainDistance = 1.5f;

    [Header("Explosion Fuse")]
    public float explosionTriggerRange = 2.5f;
    public float fuseDuration = 2f;

    [Header("Explosion")]
    public float explosionRadius = 3f;
    public float explosionDamage = 40f;
    public float explosionKnockbackStrength = 8f;
    public float explosionKnockbackDuration = 0.2f;

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
    private bool isChasing;
    private Vector2 avoidanceDirection;
    private float avoidanceTimer;

    // KNOCKBACK
    private bool isKnockedBack;
    private Vector2 knockbackVelocity;
    private float knockbackTimer;

    // FUSE
    private bool isFusing;
    private float fuseTimer;

    // DETONATION
    private bool isWaitingForAttack;
    private bool hasExploded;

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

        // HIDE COUNTDOWN INITIALLY
        if (fuseTimerText != null)
        {
            fuseTimerText.gameObject.SetActive(false);
        }

        // SUBSCRIBE TO ATTACK ANIMATION
        if (mobAttackAnimator != null)
        {
            mobAttackAnimator.AttackFinished += OnAttackFinished;
        }
    }

    private void Start()
    {
        currentHP = maxHP;
        UpdateHealthBar();

        FindPlayer();
    }

    private void Update()
    {
        if (isDead || hasExploded)
            return;

        HandleDetection();

        if (isFusing)
        {
            HandleFuse();
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || isDead || hasExploded || isWaitingForAttack)
            return;

        if (isKnockedBack)
        {
            HandleKnockback();
            return;
        }

        if (player == null || !isChasing)
            return;

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
        if (!isChasing && distance <= detectionRange)
        {
            isChasing = true;
        }

        // STOP CHASING IF PLAYER ESCAPES
        // FUSING MOBS ALWAYS CONTINUE CHASING
        if (
            isChasing &&
            !isFusing &&
            distance > chaseRange
        )
        {
            isChasing = false;
            return;
        }

        // START EXPLOSION FUSE
        if (
            isChasing &&
            !isFusing &&
            distance <= explosionTriggerRange
        )
        {
            StartFuse();
        }
    }

    // MOVEMENT

    private void MoveTowardsPlayer()
    {
        if (player == null)
            return;

        Vector2 direction =
            (player.position - transform.position).normalized;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // MAINTAIN DISTANCE
        if (distance <= maintainDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // KEEP CHASING
        Vector2 movementDirection = GetMovementDirection(direction);

        rb.MovePosition(
            rb.position +
            movementDirection * movementSpeed * Time.fixedDeltaTime
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

    // FUSE

    private void StartFuse()
    {
        if (isFusing)
            return;

        isFusing = true;
        fuseTimer = 0f;

        UpdateFuseTimerText();
    }

    private void HandleFuse()
    {
        fuseTimer += Time.deltaTime;

        UpdateFuseTimerText();

        if (fuseTimer >= fuseDuration)
        {
            BeginDetonation();
        }
    }

    private void UpdateFuseTimerText()
    {
        if (fuseTimerText == null)
            return;

        float remainingTime = Mathf.Max(
            0f,
            fuseDuration - fuseTimer
        );

        fuseTimerText.text = remainingTime.ToString("F1");

        if (!fuseTimerText.gameObject.activeSelf)
        {
            fuseTimerText.gameObject.SetActive(true);
        }
    }

    private void HideFuseTimerText()
    {
        if (fuseTimerText != null)
        {
            fuseTimerText.gameObject.SetActive(false);
        }
    }

    // DETONATION

    private void BeginDetonation()
    {
        if (hasExploded || isDead || isWaitingForAttack)
            return;

        // STOP MOVEMENT
        isWaitingForAttack = true;
        isChasing = false;

        rb.linearVelocity = Vector2.zero;
        HideFuseTimerText();

        // NO ANIMATOR: DETONATE IMMEDIATELY
        if (mobAttackAnimator == null)
        {
            isWaitingForAttack = false;
            Detonate();
            return;
        }

        // PLAY DETONATION ANIMATION
        bool animationStarted = mobAttackAnimator.PlayAttack();

        // FALLBACK IF ANIMATION CANNOT START
        if (!animationStarted && !mobAttackAnimator.IsPlaying)
        {
            isWaitingForAttack = false;
            Detonate();
        }
    }

    private void Detonate()
    {
        if (hasExploded || isDead)
            return;

        hasExploded = true;
        isWaitingForAttack = false;

        HideFuseTimerText();

        rb.linearVelocity = Vector2.zero;
        isChasing = false;
        isKnockedBack = false;

        ApplyExplosionDamage();

        if (smokeEffectAnimator != null)
        {
            GameObject smoke = smokeEffectAnimator.PlaySmokeDetached();

            if (smoke != null)
                Destroy(smoke, smokeEffectAnimator.SmokeDuration);
        }

        Die();
    }

    private void OnAttackFinished()
    {
        if (!isWaitingForAttack || isDead || hasExploded)
            return;

        isWaitingForAttack = false;

        // COMPLETE DETONATION
        Detonate();
    }

    // EXPLOSION DAMAGE

    private void ApplyExplosionDamage()
    {
        if (player == null || playerHealth == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > explosionRadius)
            return;

        // DAMAGE PLAYER
        playerHealth.TakeDamage(explosionDamage);

        // PLAYER KNOCKBACK
        if (playerMovement != null)
        {
            Vector2 direction =
                ((Vector2)player.position -
                 (Vector2)transform.position).normalized;

            if (direction.sqrMagnitude <= 0.001f)
                direction = Vector2.up;

            playerMovement.ApplyKnockback(
                direction,
                explosionKnockbackStrength,
                explosionKnockbackDuration
            );
        }
    }

    // HEALTH

    public void TakeDamage(
        float damage,
        Vector2 knockbackDirection,
        float knockbackStrength
    )
    {
        if (isDead || hasExploded || isWaitingForAttack)
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
        // DON'T INTERRUPT FINAL DETONATION
            if (isWaitingForAttack)
                return;

            float actualKnockback = Mathf.Max(
                strength - knockbackResistance,
                0f
            );
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

        HideFuseTimerText();

        rb.linearVelocity = Vector2.zero;

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

        // NO SMOKE ON NORMAL DEATH
        Destroy(gameObject);
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
                "ExplodingMobBehavior: DropLootController instance is missing!"
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
            explosionTriggerRange
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }

    private void OnDestroy()
    {
        if (mobAttackAnimator != null)
        {
            mobAttackAnimator.AttackFinished -= OnAttackFinished;
        }
    }
}