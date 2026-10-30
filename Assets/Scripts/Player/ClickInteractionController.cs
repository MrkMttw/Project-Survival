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
        // IGNORE CLICKS OVER HOTBAR
        if (isMouseClick && IsPointerOverHotbar())
            return;

        // IGNORE NORMAL MENU
        if (menuController != null &&
            menuController.menuCanvas != null &&
            menuController.menuCanvas.activeSelf)
        {
            return;
        }

        // IGNORE GAME OVER
        if (gameOverController != null &&
            gameOverController.IsGameOver())
        {
            return;
        }

        // PROJECTILE WEAPON
        if (isMouseClick &&
            projectileWeaponController != null &&
            projectileWeaponController.HasProjectileWeaponEquipped())
        {
            projectileWeaponController.TryShoot();
            return;
        }

        // MELEE WEAPON
        if (meleeWeaponController != null &&
            meleeWeaponController.HasMeleeWeaponEquipped())
        {
            // PRIORITIZE BREAKING IF THE TARGET IS VALID
            if (breakSystem != null &&
                breakSystem.CanBreakCurrentTarget())
            {
                breakSystem.TryBreak();
                return;
            }

            // OTHERWISE ATTACK
            meleeWeaponController.TryAttack();
            return;
        }

        // NO MELEE WEAPON: ATTEMPT BREAKING
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