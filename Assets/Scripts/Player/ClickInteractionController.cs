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
    
    private void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleInteraction(true);
        }

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HandleInteraction(false);
        }
    }

    private void HandleInteraction(bool isMouseClick)
    {
        // Ignore mouse clicks over UI.
        // Spacebar remains usable while interacting with UI.

        if (isMouseClick && IsPointerOverHotbar())
            return;

        // Ignore interactions while the normal menu is open.
        if (menuController != null &&
            menuController.menuCanvas != null &&
            menuController.menuCanvas.activeSelf)
        {
            return;
        }

        // Ignore interactions during Game Over.
        if (gameOverController != null &&
            gameOverController.IsGameOver())
        {
            return;
        }

        // Projectile weapon takes priority.
        if (isMouseClick &&
            projectileWeaponController != null &&
            projectileWeaponController.HasProjectileWeaponEquipped())
        {
            projectileWeaponController.TryShoot();
            return;
        }

        // Melee weapon takes priority.
        if (meleeWeaponController != null &&
            meleeWeaponController.HasMeleeWeaponEquipped())
        {
            meleeWeaponController.TryAttack();
            return;
        }

        // No weapon equipped: attempt breaking.
        if (breakSystem != null)
        {
            breakSystem.TryBreak();
        }
    }

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