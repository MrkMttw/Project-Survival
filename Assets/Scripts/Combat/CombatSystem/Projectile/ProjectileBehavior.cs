using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProjectileBehavior : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float maxDistance = 10f;
    public float maxLifetime = 3f;

    [Header("Damage")]
    public float damage = 5f;
    public float knockbackStrength = 5f;

    private Collider2D[] shooterColliders;
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
        float projectileKnockback
    )
    {
        direction = shootDirection.normalized;
        attacker = shooter;

        if (attacker != null)
        {
            shooterColliders =
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

        if (distanceTraveled >= maxDistance ||
            lifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || hasHit)
            return;

        // Ignore the shooter and their child colliders.
        if (attacker != null &&
            (other.transform == attacker ||
             other.transform.IsChildOf(attacker)))
        {
            return;
        }

        EnemyController enemy =
            other.GetComponent<EnemyController>();

        if (enemy == null)
        {
            enemy = other.GetComponentInParent<EnemyController>();
        }

        if (enemy != null)
        {
            hasHit = true;

            Vector2 knockbackDirection =
                ((Vector2)enemy.transform.position -
                 (Vector2)transform.position).normalized;

            enemy.TakeDamage(
                damage,
                knockbackDirection,
                knockbackStrength
            );

            Destroy(gameObject);
            return;
        }

        PassiveMobController passiveMob =
            other.GetComponent<PassiveMobController>();

        if (passiveMob == null)
        {
            passiveMob =
                other.GetComponentInParent<PassiveMobController>();
        }

        if (passiveMob != null)
        {
            hasHit = true;

            Vector2 knockbackDirection =
                ((Vector2)passiveMob.transform.position -
                 (Vector2)transform.position).normalized;

            passiveMob.TakeDamage(damage);
            passiveMob.ApplyKnockback(
                knockbackDirection,
                knockbackStrength
            );

            if (attacker != null)
                passiveMob.ReactToAttack(attacker);

            Destroy(gameObject);
            return;
        }

        // Other colliders block the projectile.
        hasHit = true;
        Destroy(gameObject);
    }
}