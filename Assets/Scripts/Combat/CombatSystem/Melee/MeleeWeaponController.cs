using UnityEngine;

public class MeleeWeaponController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerHeldItem playerHeldItem;

    [Header("Combat")]
    public LayerMask enemyLayer;

    [Header("Unarmed")]
    public WeaponData unarmedWeapon;

    private float nextAttackTime;

    private void Start()
    {
        if (playerHeldItem == null && player != null)
        {
            playerHeldItem = player.GetComponent<PlayerHeldItem>();
        }

        if (playerHeldItem == null)
        {
            Debug.LogError("MeleeWeaponController: PlayerHeldItem is not assigned or found!");
        }

        if (player == null)
        {
            Debug.LogError("MeleeWeaponController: Player is not assigned!");
        }

        if (unarmedWeapon == null)
        {
            Debug.LogError("MeleeWeaponController: Unarmed WeaponData is not assigned!");
        }

        if (enemyLayer.value == 0)
        {
            Debug.LogWarning("MeleeWeaponController: Enemy Layer is set to Nothing!");
        }
    }

    public bool HasMeleeWeaponEquipped()
    {
        if (playerHeldItem == null)
            return false;

        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
            return true;

        return heldItem.GetComponent<MeleeWeaponFunction>() != null;
    }

    public void TryAttack()
    {
        WeaponData weapon = GetCurrentWeapon();

        if (weapon == null)
            return;

        TryAttack(weapon);
    }

    // CHECK ENEMY IN ATTACK RANGE
    public bool HasEnemyInAttackRange()
    {
        WeaponData weapon = GetCurrentWeapon();

        if (weapon == null || player == null)
            return false;

        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                player.position,
                weapon.attackRange,
                enemyLayer
            );

        foreach (Collider2D enemyCollider in enemies)
        {
            EnemyController enemy =
                enemyCollider.GetComponentInParent<EnemyController>();

            PassiveMobController passiveMob =
                enemyCollider.GetComponentInParent<PassiveMobController>();

            if (enemy != null || passiveMob != null)
                return true;
        }

        return false;
    }

    // CURRENT WEAPON

    private WeaponData GetCurrentWeapon()
    {
        if (playerHeldItem == null)
            return null;

        Item heldItem = playerHeldItem.GetHeldItem();

        // UNARMED ATTACK
        if (heldItem == null)
            return unarmedWeapon;

        MeleeWeaponFunction weaponFunction =
            heldItem.GetComponent<MeleeWeaponFunction>();

        if (weaponFunction == null)
            return null;

        if (weaponFunction.weaponData == null)
        {
            Debug.LogWarning(
                "MeleeWeaponController: Held item '" +
                heldItem.name +
                "' has MeleeWeaponFunction but no WeaponData."
            );

            return null;
        }

        return weaponFunction.weaponData;
    }

    // ATTACK

    private bool TryAttack(WeaponData weapon)
    {
        if (weapon == null)
            return false;

        if (Time.time < nextAttackTime)
            return false;

        bool hitEnemy = Attack(weapon);

        if (hitEnemy)
        {
            Item heldItem = playerHeldItem.GetHeldItem();

            // WEAPON DURABILITY
            if (heldItem != null &&
                heldItem.GetComponent<MeleeWeaponFunction>() != null)
            {
                heldItem.UseDurability();
            }

            float attackCooldown =
                1f / Mathf.Max(weapon.attackSpeed, 0.01f);

            nextAttackTime = Time.time + attackCooldown;
        }

        return hitEnemy;
    }

    private bool Attack(WeaponData weapon)
    {
        if (player == null)
            return false;

        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                player.position,
                weapon.attackRange,
                enemyLayer
            );

        bool hitEnemy = false;

        foreach (Collider2D enemyCollider in enemies)
        {
            EnemyController enemy =
                enemyCollider.GetComponent<EnemyController>();

            if (enemy == null)
            {
                enemy =
                    enemyCollider.GetComponentInParent<EnemyController>();
            }

            if (enemy != null)
            {
                hitEnemy = true;

                float damage = Random.Range(
                    weapon.minDamage,
                    weapon.maxDamage
                );

                Vector2 knockbackDirection =
                    (
                        enemy.transform.position -
                        player.position
                    ).normalized;

                enemy.TakeDamage(
                    damage,
                    knockbackDirection,
                    weapon.knockbackStrength
                );

                continue;
            }

            PassiveMobController passiveMob =
                enemyCollider.GetComponent<PassiveMobController>();

            if (passiveMob == null)
            {
                passiveMob =
                    enemyCollider.GetComponentInParent<PassiveMobController>();
            }

            if (passiveMob != null)
            {
                hitEnemy = true;

                float damage = Random.Range(
                    weapon.minDamage,
                    weapon.maxDamage
                );

                passiveMob.TakeDamage(damage);
                passiveMob.ReactToAttack(player);
            }
        }

        return hitEnemy;
    }

    // ATTACK RANGE GIZMO

    private void OnDrawGizmosSelected()
    {
        WeaponData weapon = GetCurrentWeapon();

        if (player == null || weapon == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            player.position,
            weapon.attackRange
        );
    }
}