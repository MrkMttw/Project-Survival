using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteractionController : MonoBehaviour
{
    [Header("Interaction References")]
    public CraftingController craftingController;
    public GateController gateController;
    public FoodController foodController;
    public ItemLightController itemLightController;
    public ItemsGeneratorController itemsGeneratorController;
    public FurnaceController furnaceController;
    public ContainerController containerController;

    [Header("Sleeping")]
    public SleepTransitionController sleepTransitionController;
    public SleepingBagHitbox sleepingBagHitbox;

    [Header("Mobile Interaction Button")]
    public Image mobileInteractionButton;
    public Color highlightColor = Color.yellow;
    public float blinkSpeed = 0.6f;

    private Color defaultButtonColor;

    private void Start()
    {
        if (mobileInteractionButton != null)
        {
            defaultButtonColor =
                mobileInteractionButton.color;
        }
    }

    private void Update()
    {
        UpdateFoodPrompt();
        UpdateInteractionButtonHighlight();

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            HandleInteraction();
        }
    }

    // UPDATE FOOD PROMPT
    private void UpdateFoodPrompt()
    {
        if (PromptController.Instance == null ||
            foodController == null)
            return;

        if (foodController.CanEatHeldItem())
        {
            PromptController.Instance.RequestPrompt(
                PromptType.E, this);
        }
        else
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }
    }

    // UPDATE MOBILE BUTTON HIGHLIGHT
    private void UpdateInteractionButtonHighlight()
    {
        if (mobileInteractionButton == null)
            return;

        bool canInteract =
            PromptController.Instance != null &&
            PromptController.Instance.IsPromptActive(
                PromptType.E);

        if (!canInteract)
        {
            mobileInteractionButton.color =
                defaultButtonColor;

            return;
        }

        float pulse = Mathf.PingPong(
            Time.unscaledTime * blinkSpeed,
            1f
        );

        mobileInteractionButton.color =
            Color.Lerp(
                defaultButtonColor,
                highlightColor,
                pulse
            );
    }

    // MOBILE INTERACTION BUTTON
    public void OnMobileInteractionPressed()
    {
        HandleInteraction();
    }

    // HANDLE INTERACTION
    private void HandleInteraction()
    {
        if (craftingController != null &&
            craftingController.TryInteract())
            return;

        if (gateController != null &&
            gateController.TryInteract())
            return;

        if (furnaceController != null &&
            furnaceController.TryInteract())
            return;

        if (containerController != null &&
            containerController.TryInteract())
            return;

        if (sleepingBagHitbox != null &&
            sleepingBagHitbox.TryInteract())
            return;

        if (foodController != null &&
            foodController.TryEatHeldItem())
            return;

        if (itemLightController != null &&
            itemLightController.TryToggleHeldItemLight())
            return;

        if (itemsGeneratorController != null &&
            itemsGeneratorController.TryInteract())
            return;
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E,
                this
            );
        }

        if (mobileInteractionButton != null)
        {
            mobileInteractionButton.color =
                defaultButtonColor;
        }
    }
}