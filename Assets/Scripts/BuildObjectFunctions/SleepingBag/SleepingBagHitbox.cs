using UnityEngine;

public class SleepingBagHitbox : MonoBehaviour
{
    public PlayerInteractionController playerInteractionController;

    private SleepingBagInteraction currentSleepingBag;

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
}