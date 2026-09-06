using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerHeldItem playerHeldItem;

    [Header("Combat")]
    public LayerMask enemyLayer;

    [Header("Interaction")]
    public GameObject hotbarPanel;
    public BreakSystem breakSystem;

    private float nextAttackTime;

    private void Start()
    {
        // Automatically find PlayerHeldItem if not assigned.
        if (playerHeldItem == null && player != null)
        {
            playerHeldItem = player.GetComponent<PlayerHeldItem>();
        }

        if (playerHeldItem == null)
        {
            Debug.LogError(
                "WeaponController: PlayerHeldItem is not assigned or found!"
            );
        }

        // Automatically find BreakSystem if it's on the same object.
        // Otherwise, assign it manually in the Inspector.
        if (breakSystem == null)
        {
            breakSystem = GetComponent<BreakSystem>();
        }

        if (breakSystem == null)
        {
            Debug.LogError(
                "WeaponController: BreakSystem is not assigned!"
            );
        }

        if (player == null)
        {
            Debug.LogError(
                "WeaponController: Player is not assigned!"
            );
        }

        if (enemyLayer.value == 0)
        {
            Debug.LogWarning(
                "WeaponController: Enemy Layer is set to Nothing!"
            );
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        // Ignore clicks on the hotbar.
        if (IsPointerOverHotbar())
            return;

        HandleLeftClick();
    }

    private void HandleLeftClick()
    {
        WeaponData weapon = GetCurrentWeapon();

        // No weapon equipped.
        if (weapon == null)
        {
            TryBreak();
            return;
        }

        // Weapon is equipped.
        bool hitEnemy = TryAttack(weapon);

        // If no enemy was hit, let the BreakSystem handle the click.
        if (!hitEnemy)
        {
            TryBreak();
        }
    }

    // CURRENT WEAPON

    private WeaponData GetCurrentWeapon()
    {
        if (playerHeldItem == null)
            return null;

        Item heldItem = playerHeldItem.GetHeldItem();

        if (heldItem == null)
            return null;

        WeaponFunction weaponFunction =
            heldItem.GetComponent<WeaponFunction>();

        if (weaponFunction == null)
            return null;

        if (weaponFunction.weaponData == null)
        {
            Debug.LogWarning(
                "WeaponController: Held item '" +
                heldItem.name +
                "' has WeaponFunction but no WeaponData."
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
            float attackCooldown =
                1f / Mathf.Max(weapon.attackSpeed, 0.01f);

            nextAttackTime =
                Time.time + attackCooldown;
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

            if (enemy == null)
                continue;

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

            Debug.Log(
                "Hit " +
                enemy.name +
                " | Damage: " +
                damage.ToString("F1") +
                " | Knockback: " +
                weapon.knockbackStrength.ToString("F1")
            );
        }

        return hitEnemy;
    }

    // BREAK SYSTEM

    private void TryBreak()
    {
        if (breakSystem == null)
            return;

        breakSystem.TryBreak();
    }

    // HOTBAR UI

    private bool IsPointerOverHotbar()
    {
        if (hotbarPanel == null)
            return false;

        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData =
            new PointerEventData(EventSystem.current);

        pointerData.position =
            Mouse.current.position.ReadValue();

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            pointerData,
            results
        );

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.transform.IsChildOf(
                hotbarPanel.transform))
            {
                return true;
            }
        }

        return false;
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