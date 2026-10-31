using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Interaction References")]
    public GateController gateController;
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

        // FOOD PROMPT
        UpdateFoodPrompt();

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    // UPDATE FOOD PROMPT
    private void UpdateFoodPrompt()
    {
        if (PromptController.Instance == null || foodController == null)
            return;

        if (foodController.CanEatHeldItem())
        {
            PromptController.Instance.RequestPrompt(PromptType.E, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(PromptType.E, this);
        }
    }

    private void HandleInteraction()
    {
        if (gateController != null &&
            gateController.TryInteract())
        {
            return;
        }

        if (furnaceController != null &&
            furnaceController.TryInteract())
        {
            return;
        }

        if (sleepingBagHitbox != null &&
            sleepingBagHitbox.TryInteract())
        {
            return;
        }

        if (foodController != null &&
            foodController.TryEatHeldItem())
        {
            return;
        }

        if (itemLightController != null &&
            itemLightController.TryToggleHeldItemLight())
        {
            return;
        }

        if (itemsGeneratorController != null &&
            itemsGeneratorController.TryInteract())
        {
            return;
        }
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(PromptType.E, this);
        }
    }
}