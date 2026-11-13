using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ClickInteractionController : MonoBehaviour
{
    [Header("Interaction Controllers")]
    public ProjectileWeaponController projectileWeaponController;
    public MeleeWeaponController meleeWeaponController;
    public BreakSystem breakSystem;

    [Header("UI")]
    public MenuController menuController;
    public GameOverController gameOverController;
    public GameObject hotbarPanel;

    // MOBILE BASIC ATTACK
    public void OnMobileAttackPressed()
    {
        HandleMeleeAndBreaking();
    }

    private void Update()
    {
        // PROJECTILE SHOOTING ONLY
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleProjectileShooting();
        }

        // MELEE ATTACK / BREAKING
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HandleMeleeAndBreaking();
        }
    }

    // PROJECTILE SHOOTING
    private void HandleProjectileShooting()
    {
        if (IsInputBlocked())
            return;

        // IGNORE CLICKS OVER HOTBAR
        if (IsPointerOverHotbar())
            return;

        if (projectileWeaponController != null &&
            projectileWeaponController.HasProjectileWeaponEquipped())
        {
            projectileWeaponController.TryShoot();
        }
    }

    // MELEE ATTACK / BREAKING
    private void HandleMeleeAndBreaking()
    {
        if (IsInputBlocked())
            return;

        // MELEE WEAPON
        if (meleeWeaponController != null &&
            meleeWeaponController.HasMeleeWeaponEquipped())
        {
            // PRIORITY 1: ATTACK ENEMIES
            if (meleeWeaponController.HasEnemyInAttackRange())
            {
                meleeWeaponController.TryAttack();
                return;
            }

            // PRIORITY 2: BREAK OBJECTS
            if (breakSystem != null &&
                breakSystem.CanBreakCurrentTarget())
            {
                breakSystem.TryBreak();
                return;
            }

            return;
        }

        // NO MELEE WEAPON: ATTEMPT BREAKING
        if (breakSystem != null)
        {
            breakSystem.TryBreak();
        }
    }

    // INPUT RESTRICTIONS
    private bool IsInputBlocked()
    {
        // IGNORE NORMAL MENU
        if (menuController != null &&
            menuController.menuCanvas != null &&
            menuController.menuCanvas.activeSelf)
        {
            return true;
        }

        // IGNORE GAME OVER
        if (gameOverController != null &&
            gameOverController.IsGameOver())
        {
            return true;
        }

        return false;
    }

    // HOTBAR CLICK DETECTION
    private bool IsPointerOverHotbar()
    {
        if (hotbarPanel == null)
            return false;

        if (EventSystem.current == null ||
            Mouse.current == null)
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
}