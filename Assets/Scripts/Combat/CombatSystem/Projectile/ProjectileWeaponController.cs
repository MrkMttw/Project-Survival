using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileWeaponController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerHeldItem playerHeldItem;

    [Header("Inventory")]
    public HotbarController hotbarController;

    private float nextAttackTime;

    private void Start()
    {
        if (playerHeldItem == null && player != null)
        {
            playerHeldItem = player.GetComponent<PlayerHeldItem>();
        }

        if (playerHeldItem == null)
        {
            Debug.LogError(
                "ProjectileWeaponController: PlayerHeldItem is missing!"
            );
        }

        if (player == null)
        {
            Debug.LogError(
                "ProjectileWeaponController: Player is missing!"
            );
        }

        if (hotbarController == null)
        {
            Debug.LogError(
                "ProjectileWeaponController: HotbarController is missing!"
            );
        }
    }

    public bool HasProjectileWeaponEquipped()
    {
        if (playerHeldItem == null)
            return false;

        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
            return false;

        return heldItem.GetComponent<ProjectileWeaponFunction>() != null;
    }

    public void TryShoot()
    {
        if (player == null || Mouse.current == null)
            return;

        ProjectileWeaponData weapon = GetCurrentWeapon();

        if (weapon == null)
            return;

        // Check cooldown.
        if (Time.time < nextAttackTime)
            return;

        // Validate weapon configuration.
        if (weapon.projectilePrefab == null ||
            weapon.ammunitionItem == null)
        {
            Debug.LogWarning(
                "ProjectileWeaponController: Projectile or ammunition is missing!"
            );
            return;
        }

        Item ammunition =
            weapon.ammunitionItem.GetComponent<Item>();

        if (ammunition == null)
        {
            Debug.LogWarning(
                "ProjectileWeaponController: Ammunition prefab has no Item component!"
            );
            return;
        }

        // Calculate mouse position in world space.
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        Vector2 direction =
            (mousePosition - (Vector2)player.position).normalized;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        // Consume ammunition before shooting.
        if (!ConsumeAmmunition(ammunition.ID))
            return;

        // Calculate projectile rotation.
        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        Quaternion rotation =
            Quaternion.Euler(0f, 0f, angle - 45);

        // Calculate damage.
        float damage = Random.Range(
            weapon.minDamage,
            weapon.maxDamage + 1
        );

        // Spawn projectile.
        Vector2 spawnOffset = new Vector2(0f, 0.3f);

        Vector2 spawnPosition =
            (Vector2)player.position + spawnOffset;

        GameObject projectile = Instantiate(
            weapon.projectilePrefab,
            spawnPosition,
            rotation
        );

        ProjectileBehavior projectileBehavior =
            projectile.GetComponent<ProjectileBehavior>();

        if (projectileBehavior == null)
        {
            Debug.LogError(
                "ProjectileWeaponController: Projectile prefab has no ProjectileBehavior!"
            );

            Destroy(projectile);
            return;
        }

        projectileBehavior.Initialize(
            direction,
            player,
            weapon.projectileSpeed,
            weapon.maxDistance,
            weapon.maxLifetime,
            damage,
            weapon.knockbackStrength
        );

        // Apply cooldown after a successful shot.
        float cooldown =
            1f / Mathf.Max(weapon.attackSpeed, 0.01f);

        nextAttackTime = Time.time + cooldown;
    }

    private bool ConsumeAmmunition(int itemID)
    {
        // Check hotbar first.
        if (hotbarController != null)
        {
            int removed =
                hotbarController.RemoveItem(itemID, 1);

            if (removed > 0)
                return true;
        }

        // Check inventory second.
        if (InventoryController.instance != null)
        {
            int removed =
                InventoryController.instance.RemoveItem(itemID, 1);

            if (removed > 0)
                return true;
        }

        return false;
    }

    private ProjectileWeaponData GetCurrentWeapon()
    {
        if (playerHeldItem == null)
            return null;

        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
            return null;

        ProjectileWeaponFunction weaponFunction =
            heldItem.GetComponent<ProjectileWeaponFunction>();

        if (weaponFunction == null)
            return null;

        if (weaponFunction.weaponData == null)
        {
            Debug.LogWarning(
                "ProjectileWeaponController: Missing ProjectileWeaponData on " +
                heldItem.name
            );

            return null;
        }

        return weaponFunction.weaponData;
    }
}