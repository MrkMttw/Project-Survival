using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MobProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float maxDistance = 10f;
    public float maxLifetime = 3f;

    [Header("Damage")]
    public float damage = 5f;
    public float knockbackStrength = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Hit Settings")]
    public float hitDestroyDelay = 0.3f;

    private Vector2 direction;
    private Vector2 startPosition;
    private Transform attacker;

    private float lifetime;
    private bool initialized;
    private bool hasHit;

    public void Initialize(
        Vector2 shootDirection,
        Transform shooter,
        float projectileSpeed,
        float projectileMaxDistance,
        float projectileMaxLifetime,
        float projectileDamage,
        float projectileKnockback,
        float projectileKnockbackDuration
    )
    {
        direction = shootDirection.normalized;
        attacker = shooter;

        // IGNORE SHOOTER COLLISIONS
        if (attacker != null)
        {
            Collider2D[] shooterColliders =
                attacker.GetComponentsInChildren<Collider2D>();

            Collider2D[] projectileColliders =
                GetComponents<Collider2D>();

            foreach (Collider2D shooterCollider in shooterColliders)
            {
                foreach (Collider2D projectileCollider in projectileColliders)
                {
                    Physics2D.IgnoreCollision(
                        projectileCollider,
                        shooterCollider
                    );
                }
            }
        }

        speed = projectileSpeed;
        maxDistance = projectileMaxDistance;
        maxLifetime = projectileMaxLifetime;

        damage = projectileDamage;
        knockbackStrength = projectileKnockback;
        knockbackDuration = projectileKnockbackDuration;

        startPosition = transform.position;
        lifetime = 0f;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || hasHit)
            return;

        lifetime += Time.deltaTime;

        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        float distanceTraveled = Vector2.Distance(
            startPosition,
            transform.position
        );

        if (distanceTraveled >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }

        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || hasHit)
            return;

        // IGNORE SHOOTER
        if (
            attacker != null &&
            (other.transform == attacker ||
            other.transform.IsChildOf(attacker))
        )
        {
            return;
        }

        // FIND PLAYER USING THE SAME METHOD AS EXPLODING MOB
        GameObject[] players =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObject in players)
        {
            // CHECK IF COLLISION BELONGS TO THIS PLAYER
            if (
                other.transform != playerObject.transform &&
                !other.transform.IsChildOf(playerObject.transform)
            )
            {
                continue;
            }

            HealthController playerHealth =
                playerObject.GetComponentInChildren<HealthController>(true);

            PlayerMovement playerMovement =
                playerObject.GetComponentInChildren<PlayerMovement>(true);

            hasHit = true;

            // DISABLE PROJECTILE COLLIDER
            Collider2D projectileCollider =
                GetComponent<Collider2D>();

            if (projectileCollider != null)
                projectileCollider.enabled = false;

            // APPLY DAMAGE
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning(
                    "MobProjectile: HealthController not found under " +
                    playerObject.name
                );
            }

            // APPLY KNOCKBACK
            if (playerMovement != null)
            {
                Vector2 knockbackDirection =
                    ((Vector2)playerObject.transform.position -
                    (Vector2)transform.position).normalized;

                if (knockbackDirection.sqrMagnitude <= 0.001f)
                    knockbackDirection = direction;

                playerMovement.ApplyKnockback(
                    knockbackDirection,
                    knockbackStrength,
                    knockbackDuration
                );
            }

            Destroy(gameObject, hitDestroyDelay);
            return;
        }

        // OTHER COLLIDERS BLOCK PROJECTILE
        hasHit = true;

        Debug.LogWarning(
            "MobProjectile: Projectile blocked by " + other.name +
            " | Layer: " + LayerMask.LayerToName(other.gameObject.layer)
        );

        Destroy(gameObject, hitDestroyDelay);
    }
}