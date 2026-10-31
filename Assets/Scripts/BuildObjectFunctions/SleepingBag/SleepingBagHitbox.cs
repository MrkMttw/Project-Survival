using UnityEngine;

public class SleepingBagHitbox : MonoBehaviour
{
    public PlayerInteractionController playerInteractionController;

    private SleepingBagInteraction currentSleepingBag;

    private void Update()
    {
        // UPDATE SLEEPING BAG PROMPT
        if (PromptController.Instance == null)
            return;

        if (currentSleepingBag != null)
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

    private void OnTriggerStay2D(Collider2D other)
    {
        SleepingBagInteraction sleepingBag =
            other.GetComponentInParent<SleepingBagInteraction>();

        if (sleepingBag != null)
        {
            currentSleepingBag = sleepingBag;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        SleepingBagInteraction sleepingBag =
            other.GetComponentInParent<SleepingBagInteraction>();

        if (sleepingBag != null &&
            currentSleepingBag == sleepingBag)
        {
            currentSleepingBag = null;
        }
    }

    public bool TryInteract()
    {
        if (currentSleepingBag == null)
            return false;

        if (playerInteractionController == null)
            return false;

        currentSleepingBag.Interact(
            playerInteractionController.sleepTransitionController
        );

        return true;
    }

    private void OnDisable()
    {
        if (PromptController.Instance != null)
        {
            PromptController.Instance.ReleasePrompt(
                PromptType.E, this);
        }

        currentSleepingBag = null;
    }
}