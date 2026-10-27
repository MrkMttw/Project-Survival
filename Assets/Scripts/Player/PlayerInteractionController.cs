using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Interaction References")]
    public FoodController foodController;
    public ItemLightController itemLightController;
    public ItemsGeneratorController itemsGeneratorController;
    public FurnaceController furnaceController;
    
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
        Debug.Log("E pressed - checking interactions.");
        if (furnaceController != null && furnaceController.TryInteract())
            return;
            
        if (sleepingBagHitbox != null &&
            sleepingBagHitbox.TryInteract())
        {
            Debug.Log("Interaction: Sleeping bag.");
            return;
        }

        if (foodController != null &&
            foodController.TryEatHeldItem())
        {
            Debug.Log("Interaction: Ate food.");
            return;
        }

        if (itemLightController != null &&
            itemLightController.TryToggleHeldItemLight())
        {
            Debug.Log("Interaction: Toggled held item light.");
            return;
        }

        if (itemsGeneratorController != null)
        {
            bool interacted = itemsGeneratorController.TryInteract();

            if (interacted)
            {
                Debug.Log("Interaction: Item generator found.");
            }
            else
            {
                Debug.Log("No item generator interaction available.");
            }

            return;
        }

        Debug.LogWarning("ItemsGeneratorController is not assigned!");
    }
}