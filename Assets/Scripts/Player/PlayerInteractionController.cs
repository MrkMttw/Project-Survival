using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Interaction References")]
    public FoodController foodController;
    public ItemLightController itemLightController;

    [Header("Sleeping")]
    public SleepTransitionController sleepTransitionController;
    public SleepingBagHitbox sleepingBagHitbox;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // Sleeping bag interaction takes priority when nearby.
        if (sleepingBagHitbox != null &&
            sleepingBagHitbox.TryInteract())
        {
            return;
        }

        // Try eating first if the held item is food.
        if (foodController != null &&
            foodController.TryEatHeldItem())
        {
            return;
        }

        // Otherwise, try toggling the held item's light.
        if (itemLightController != null)
        {
            itemLightController.TryToggleHeldItemLight();
        }
    }
}